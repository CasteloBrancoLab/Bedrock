using Bedrock.BuildingBlocks.Application.UseCases.Interfaces;
using Bedrock.BuildingBlocks.Application.UseCases.Models;
using Bedrock.BuildingBlocks.Observability.ExtensionMethods;
using Microsoft.Extensions.Logging;

namespace Bedrock.BuildingBlocks.Application.UseCases;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: UseCaseBase - Template Method Pattern para Use Cases
═══════════════════════════════════════════════════════════════════════════════

UseCaseBase implementa o Template Method Pattern para fornecer comportamento
consistente de tratamento de erros em use cases da camada Application.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Método Público Encapsula Tratamento de Erros
───────────────────────────────────────────────────────────────────────────────

O método público ExecuteAsync:
✅ Chama ConfigureExecutionInternal (lazy, uma vez)
✅ Delega para UnitOfWork se configurado, senão executa direto
✅ Captura exceções e registra com contexto de tracing
✅ Retorna null em caso de falha (graceful degradation)

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Método Abstrato Interno Para Implementação Concreta
───────────────────────────────────────────────────────────────────────────────

O método público tem um correspondente *InternalAsync protegido e abstrato:
- ExecuteAsync → ExecuteInternalAsync

Implementações concretas DEVEM implementar APENAS ExecuteInternalAsync,
nunca o método público.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: ConfigureExecutionInternal - Lazy Initialization
───────────────────────────────────────────────────────────────────────────────

ConfigureExecutionInternal é chamado na PRIMEIRA execução de ExecuteAsync,
não no construtor. Isso garante que campos DI da classe filha já estejam
inicializados (C# inicializa campos DEPOIS de base() retornar).

═══════════════════════════════════════════════════════════════════════════════
*/

/// <summary>
/// Abstract base class for use case implementations that provides consistent error handling,
/// optional unit of work orchestration, and distributed tracing integration using the Template Method pattern.
/// </summary>
/// <remarks>
/// <para>
/// This class implements <see cref="IUseCase{TInput, TOutput}"/> and provides a uniform
/// error handling strategy. The public <see cref="ExecuteAsync"/> method wraps the abstract
/// <see cref="ExecuteInternalAsync"/> in a try-catch block that logs exceptions with
/// distributed tracing context and returns null.
/// </para>
/// <para>
/// Concrete implementations override <see cref="ConfigureExecutionInternal"/> to set up
/// execution options (e.g., unit of work) and <see cref="ExecuteInternalAsync"/> for business logic.
/// Configuration is lazy-initialized on the first call to <see cref="ExecuteAsync"/> to ensure
/// that DI-injected fields in derived classes are already initialized.
/// </para>
/// </remarks>
/// <typeparam name="TInput">The type of the use case input. Must be a reference type.</typeparam>
/// <typeparam name="TOutput">The type of the use case output. Must be a reference type.</typeparam>
public abstract class UseCaseBase<TInput, TOutput>
    : IUseCase<TInput, TOutput>
    where TInput : class
    where TOutput : class
{
    private bool _configured;

    /// <summary>
    /// Gets the logger instance for this use case.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Gets the execution options configured by <see cref="ConfigureExecutionInternal"/>.
    /// </summary>
    protected UseCaseExecutionOptions ExecutionOptions { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UseCaseBase{TInput, TOutput}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for recording operations and errors.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    protected UseCaseBase(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        Logger = logger;
        ExecutionOptions = new UseCaseExecutionOptions();
    }

    /// <inheritdoc />
    public async Task<TOutput?> ExecuteAsync(
        ExecutionContext executionContext,
        TInput input,
        CancellationToken cancellationToken)
    {
        if (!_configured)
        {
            ConfigureExecutionInternal(ExecutionOptions);
            _configured = true;
        }

        try
        {
            if (ExecutionOptions.UnitOfWork is not null)
                return await ExecuteWithUnitOfWorkAsync(executionContext, input, cancellationToken);

            return await ExecuteInternalAsync(executionContext, input, cancellationToken);
        }
        catch (Exception ex)
        {
            // Stryker disable once String : Log message content is not behavior-critical
            Logger.LogExceptionForDistributedTracing(
                executionContext,
                ex,
                "An error occurred while executing use case.");
            return null;
        }
    }

    /// <summary>
    /// When overridden in a derived class, configures execution options such as unit of work.
    /// Called once (lazily) before the first execution.
    /// </summary>
    /// <param name="options">The execution options to configure.</param>
    protected abstract void ConfigureExecutionInternal(UseCaseExecutionOptions options);

    /// <summary>
    /// When overridden in a derived class, executes the use case business logic.
    /// </summary>
    /// <param name="executionContext">The execution context for multi-tenancy and audit.</param>
    /// <param name="input">The use case input data.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The use case output if successful; otherwise, null.</returns>
    protected abstract Task<TOutput?> ExecuteInternalAsync(
        ExecutionContext executionContext,
        TInput input,
        CancellationToken cancellationToken);

    private async Task<TOutput?> ExecuteWithUnitOfWorkAsync(
        ExecutionContext executionContext,
        TInput input,
        CancellationToken cancellationToken)
    {
        TOutput? capturedResult = null;

        bool success = await ExecutionOptions.UnitOfWork!.ExecuteAsync(
            executionContext,
            input,
            async (ctx, inp, ct) =>
            {
                capturedResult = await ExecuteInternalAsync(ctx, inp, ct);
                return capturedResult is not null;
            },
            cancellationToken);

        return success ? capturedResult : null;
    }
}
