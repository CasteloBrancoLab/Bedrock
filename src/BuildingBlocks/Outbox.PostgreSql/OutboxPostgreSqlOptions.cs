namespace Bedrock.BuildingBlocks.Outbox.PostgreSql;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: OutboxPostgreSqlOptions - Configuracao da Tabela Outbox
═══════════════════════════════════════════════════════════════════════════════

Segue o padrao de Options do projeto: sealed class com private set e fluent API.
A base class (OutboxPostgreSqlRepositoryBase) instancia este Options e chama
o metodo abstrato ConfigureInternal para que cada BC configure schema, tabela
e max retries.

Exemplo de uso numa classe concreta:
    protected override void ConfigureInternal(OutboxPostgreSqlOptions options)
    {
        options
            .WithSchema("auth")
            .WithTableName("outbox")
            .WithMaxRetries(3);
    }

═══════════════════════════════════════════════════════════════════════════════
*/

/// <summary>
/// Opcoes de configuracao para o repositorio PostgreSQL da outbox.
/// </summary>
public sealed class OutboxPostgreSqlOptions
{
    /// <summary>
    /// Schema do PostgreSQL onde a tabela reside (default: "public").
    /// </summary>
    public string Schema { get; private set; } = "public";

    /// <summary>
    /// Nome da tabela de outbox (default: "outbox").
    /// </summary>
    public string TableName { get; private set; } = "outbox";

    /// <summary>
    /// Numero maximo de retries antes de transicionar para Dead (default: 5).
    /// </summary>
    public byte MaxRetries { get; private set; } = 5;

    /// <summary>
    /// Define o schema do PostgreSQL.
    /// </summary>
    public OutboxPostgreSqlOptions WithSchema(string schema)
    {
        Schema = schema;
        return this;
    }

    /// <summary>
    /// Define o nome da tabela de outbox.
    /// </summary>
    public OutboxPostgreSqlOptions WithTableName(string tableName)
    {
        TableName = tableName;
        return this;
    }

    /// <summary>
    /// Define o numero maximo de retries antes de Dead.
    /// </summary>
    public OutboxPostgreSqlOptions WithMaxRetries(byte maxRetries)
    {
        MaxRetries = maxRetries;
        return this;
    }
}
