using Bedrock.BuildingBlocks.Core.Ids;
using Templates.Domain.Repositories.Interfaces;
using Templates.Domain.Validators.Interfaces;

namespace Templates.Domain.Validators;

/*
───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Sealed + Constructor Injection (Mesmo Padrão dos Services)
───────────────────────────────────────────────────────────────────────────────

Validators seguem exatamente o mesmo padrão de Services:
✅ sealed class
✅ Constructor injection com null checks
✅ Error messages via ExecutionContext

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Message Codes Descritivos com Namespace
───────────────────────────────────────────────────────────────────────────────

Códigos de mensagem seguem o padrão "Classe.Operação":

✅ "SimpleAggregateRootExistenceValidator.NotFound"
✅ "AuthenticationService.InvalidCredentials"
❌ "NotFound" // genérico, impossível rastrear origem
❌ "error_001" // código numérico sem semântica

RAZÃO: Em logs e debugging, o código identifica exatamente QUEM gerou
a mensagem e QUAL foi o problema, sem ambiguidade.

───────────────────────────────────────────────────────────────────────────────
*/

public sealed class SimpleAggregateRootExistenceValidator : ISimpleAggregateRootExistenceValidator
{
    private const string NotFoundMessageCode = "SimpleAggregateRootExistenceValidator.NotFound";

    private readonly ISimpleAggregateRootRepository _repository;

    public SimpleAggregateRootExistenceValidator(
        ISimpleAggregateRootRepository repository
    )
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<bool> ValidateExistsAsync(
        ExecutionContext executionContext,
        Id id,
        CancellationToken cancellationToken)
    {
        bool exists = await _repository.ExistsAsync(
            executionContext,
            id,
            cancellationToken);

        if (!exists)
        {
            executionContext.AddErrorMessage(code: NotFoundMessageCode);
            return false;
        }

        return true;
    }
}
