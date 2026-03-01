using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Outbox.Interfaces;
using Bedrock.BuildingBlocks.Outbox.Models;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.UnitOfWork.Interfaces;
using NpgsqlTypes;

namespace Bedrock.BuildingBlocks.Outbox.PostgreSql;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: OutboxPostgreSqlRepositoryBase - Persistencia Outbox em PostgreSQL
═══════════════════════════════════════════════════════════════════════════════

Classe abstrata que implementa IOutboxRepository usando raw Npgsql via
IPostgreSqlUnitOfWork. Segue o padrao do projeto: base instancia Options e
chama ConfigureInternal para que cada BC configure schema, tabela e max retries.

NAO usa DataModelRepositoryBase porque OutboxEntry nao e um domain entity
e nao herda de DataModelBase (sem audit columns, sem EntityVersion).

Usa lazy initialization para SQL (ConfigureInternal chamado no primeiro uso,
NAO no construtor) — mesmo padrao de PostgreSqlConnectionBase e UseCaseBase.
Isso permite que classes derivadas usem valores injetados via construtor.

Padroes usados:
1. SQL cacheado apos primeira chamada (strings imutaveis, zero alocacao por chamada)
2. FOR UPDATE SKIP LOCKED no ClaimNextBatchAsync (concorrencia sem lock externo)
3. CTE (WITH ... UPDATE ... RETURNING) para claim atomico
4. NOW() do PostgreSQL para timestamps server-side (evita clock drift)
5. Lease pattern: IsProcessing + ProcessingExpiration para workers concorrentes
6. MaxRetries com transicao automatica para Dead no MarkAsFailedAsync e ClaimNextBatchAsync

Exemplo de implementacao concreta:
    public sealed class AuthOutboxRepository : OutboxPostgreSqlRepositoryBase
    {
        public AuthOutboxRepository(IAuthPostgreSqlUnitOfWork unitOfWork)
            : base(unitOfWork) { }

        protected override void ConfigureInternal(OutboxPostgreSqlOptions options)
        {
            options.WithSchema("auth").WithMaxRetries(3);
        }
    }

═══════════════════════════════════════════════════════════════════════════════
*/

/// <summary>
/// Base abstrata para repositorios PostgreSQL de outbox.
/// Cada bounded context herda e implementa <see cref="ConfigureInternal"/>
/// para definir schema, tabela e max retries.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Integration-test-only: requires PostgreSQL database")]
public abstract class OutboxPostgreSqlRepositoryBase : IOutboxRepository
{
    private readonly IPostgreSqlUnitOfWork _unitOfWork;

    private string _insertSql = null!;
    private string _claimSql = null!;
    private string _markSentSql = null!;
    private string _markFailedSql = null!;
    private bool _configured;

    protected OutboxPostgreSqlRepositoryBase(IPostgreSqlUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Configura as opcoes do repositorio (schema, tabela, max retries).
    /// </summary>
    protected abstract void ConfigureInternal(OutboxPostgreSqlOptions options);

    private void EnsureConfigured()
    {
        if (_configured) return;

        var options = new OutboxPostgreSqlOptions();
        ConfigureInternal(options);
        BuildSqlStatements(options);
        _configured = true;
    }

    private void BuildSqlStatements(OutboxPostgreSqlOptions options)
    {
        var table = $"{options.Schema}.{options.TableName}";

        _insertSql =
            $"""
            INSERT INTO {table}
                (id, tenant_code, correlation_id, payload_type, content_type,
                 payload, created_at, status, processed_at,
                 retry_count, is_processing, processing_expiration)
            VALUES
                (@id, @tenant_code, @correlation_id, @payload_type, @content_type,
                 @payload, @created_at, @status, @processed_at,
                 @retry_count, @is_processing, @processing_expiration)
            """;

        _claimSql =
            $"""
            WITH claimed AS (
                SELECT id, status AS prev_status FROM {table}
                WHERE (
                    (status IN ({(byte)OutboxEntryStatus.Pending}, {(byte)OutboxEntryStatus.Failed})
                     AND (is_processing = FALSE OR processing_expiration < NOW()))
                    OR
                    (status = {(byte)OutboxEntryStatus.Processing} AND processing_expiration < NOW())
                )
                ORDER BY created_at
                LIMIT @batch_size
                FOR UPDATE SKIP LOCKED
            )
            UPDATE {table} o
            SET is_processing = TRUE,
                status = CASE
                    WHEN o.retry_count + 1 >= {options.MaxRetries} AND c.prev_status = {(byte)OutboxEntryStatus.Processing}
                        THEN {(byte)OutboxEntryStatus.Dead}
                    ELSE {(byte)OutboxEntryStatus.Processing}
                END,
                retry_count = CASE
                    WHEN c.prev_status = {(byte)OutboxEntryStatus.Processing}
                        THEN o.retry_count + 1
                    ELSE o.retry_count
                END,
                processing_expiration = CASE
                    WHEN o.retry_count + 1 >= {options.MaxRetries} AND c.prev_status = {(byte)OutboxEntryStatus.Processing}
                        THEN NULL
                    ELSE NOW() + @lease_duration::interval
                END
            FROM claimed c
            WHERE o.id = c.id
            RETURNING o.id, o.tenant_code, o.correlation_id, o.payload_type, o.content_type,
                      o.payload, o.created_at, o.status, o.processed_at, o.retry_count,
                      o.is_processing, o.processing_expiration
            """;

        _markSentSql =
            $"""
            UPDATE {table}
            SET status = {(byte)OutboxEntryStatus.Sent},
                processed_at = NOW(),
                is_processing = FALSE,
                processing_expiration = NULL
            WHERE id = @id
            """;

        _markFailedSql =
            $"""
            UPDATE {table}
            SET status = CASE
                    WHEN retry_count + 1 >= {options.MaxRetries}
                        THEN {(byte)OutboxEntryStatus.Dead}
                    ELSE {(byte)OutboxEntryStatus.Failed}
                END,
                retry_count = retry_count + 1,
                is_processing = FALSE,
                processing_expiration = NULL
            WHERE id = @id
            """;
    }

    /// <inheritdoc />
    public async Task AddAsync(OutboxEntry entry, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        await using var command = _unitOfWork.CreateNpgsqlCommand(_insertSql);

        command.Parameters.AddWithValue("@id", NpgsqlDbType.Uuid, entry.Id);
        command.Parameters.AddWithValue("@tenant_code", NpgsqlDbType.Uuid, entry.TenantCode);
        command.Parameters.AddWithValue("@correlation_id", NpgsqlDbType.Uuid, entry.CorrelationId);
        command.Parameters.AddWithValue("@payload_type", NpgsqlDbType.Text, entry.PayloadType);
        command.Parameters.AddWithValue("@content_type", NpgsqlDbType.Text, entry.ContentType);
        command.Parameters.AddWithValue("@payload", NpgsqlDbType.Bytea, entry.Payload);
        command.Parameters.AddWithValue("@created_at", NpgsqlDbType.TimestampTz, entry.CreatedAt);
        command.Parameters.AddWithValue("@status", NpgsqlDbType.Smallint, (short)(byte)entry.Status);
        command.Parameters.AddWithValue("@processed_at", NpgsqlDbType.TimestampTz,
            (object?)entry.ProcessedAt ?? DBNull.Value);
        command.Parameters.AddWithValue("@retry_count", NpgsqlDbType.Smallint, (short)entry.RetryCount);
        command.Parameters.AddWithValue("@is_processing", NpgsqlDbType.Boolean, entry.IsProcessing);
        command.Parameters.AddWithValue("@processing_expiration", NpgsqlDbType.TimestampTz,
            (object?)entry.ProcessingExpiration ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OutboxEntry>> ClaimNextBatchAsync(
        int batchSize,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();
        await using var command = _unitOfWork.CreateNpgsqlCommand(_claimSql);

        command.Parameters.AddWithValue("@batch_size", NpgsqlDbType.Integer, batchSize);
        command.Parameters.AddWithValue("@lease_duration", NpgsqlDbType.Interval, leaseDuration);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var entries = new List<OutboxEntry>();

        while (await reader.ReadAsync(cancellationToken))
        {
            entries.Add(new OutboxEntry
            {
                Id = reader.GetGuid(0),
                TenantCode = reader.GetGuid(1),
                CorrelationId = reader.GetGuid(2),
                PayloadType = reader.GetString(3),
                ContentType = reader.GetString(4),
                Payload = (byte[])reader.GetValue(5),
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(6),
                Status = (OutboxEntryStatus)(byte)reader.GetInt16(7),
                ProcessedAt = reader.IsDBNull(8) ? null : reader.GetFieldValue<DateTimeOffset>(8),
                RetryCount = (byte)reader.GetInt16(9),
                IsProcessing = reader.GetBoolean(10),
                ProcessingExpiration = reader.IsDBNull(11) ? null : reader.GetFieldValue<DateTimeOffset>(11)
            });
        }

        return entries;
    }

    /// <inheritdoc />
    public async Task MarkAsSentAsync(Guid id, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        await using var command = _unitOfWork.CreateNpgsqlCommand(_markSentSql);
        command.Parameters.AddWithValue("@id", NpgsqlDbType.Uuid, id);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task MarkAsFailedAsync(Guid id, CancellationToken cancellationToken)
    {
        EnsureConfigured();
        await using var command = _unitOfWork.CreateNpgsqlCommand(_markFailedSql);
        command.Parameters.AddWithValue("@id", NpgsqlDbType.Uuid, id);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
