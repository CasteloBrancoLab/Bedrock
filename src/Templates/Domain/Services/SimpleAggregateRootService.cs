using Bedrock.BuildingBlocks.Core.BirthDates;
using Bedrock.BuildingBlocks.Core.Ids;
using Templates.Domain.Entities.SimpleAggregateRoots;
using Templates.Domain.Entities.SimpleAggregateRoots.Inputs;
using Templates.Domain.Repositories.Interfaces;
using Templates.Domain.Services.Interfaces;

namespace Templates.Domain.Services;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: Domain Service - Orquestração Entre Entidades e Repositórios
═══════════════════════════════════════════════════════════════════════════════

Domain Service implementa operações de negócio que cruzam as fronteiras de
uma única entidade. Ele orquestra: criar/modificar entidade → persistir.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Classe Sealed
───────────────────────────────────────────────────────────────────────────────

Domain Services DEVEM ser sealed:
✅ public sealed class SimpleAggregateRootService
❌ public class SimpleAggregateRootService // herança não intencional

RAZÃO: Comportamento previsível, otimização do compilador (devirtualização).
Variações de comportamento usam composição (Strategy, injeção de dependência).

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Constructor Injection com Null Checks
───────────────────────────────────────────────────────────────────────────────

Toda dependência é recebida via construtor com validação:

✅ _repository = repository ?? throw new ArgumentNullException(nameof(repository));
❌ _repository = repository; // NullReferenceException silenciosa depois

RAZÃO: Fail-fast no momento da composição (startup), não no momento do uso.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Error Messages via ExecutionContext, Não Exceptions
───────────────────────────────────────────────────────────────────────────────

Falhas de negócio são comunicadas via ExecutionContext.AddErrorMessage():

✅ executionContext.AddErrorMessage(code: "Service.NotFound");
   return null;
❌ throw new NotFoundException("Entity not found"); // controle de fluxo via exceção

RAZÃO: ExecutionContext acumula TODAS as mensagens de erro numa operação.
Exceptions interrompem no primeiro erro — UX ruim para validações compostas.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Verificar HasErrorMessages Antes de Adicionar
───────────────────────────────────────────────────────────────────────────────

Se a entidade ou repositório já adicionou mensagem de erro, não duplique:

✅ if (!executionContext.HasErrorMessages)
       executionContext.AddErrorMessage(code: "Service.RegistrationFailed");
❌ executionContext.AddErrorMessage(code: "Service.RegistrationFailed"); // pode duplicar

RAZÃO: A entidade pode ter adicionado mensagens mais específicas
(ex: "FirstName is required"). A mensagem genérica do service é fallback.

───────────────────────────────────────────────────────────────────────────────
LLM_TEMPLATE: Padrão de Operação de Escrita
───────────────────────────────────────────────────────────────────────────────

1. Criar/modificar entidade via factory method ou método de negócio
2. Verificar null (falha de validação na entidade)
3. Persistir via repositório
4. Verificar resultado da persistência
5. Retornar entidade ou null

var entity = Entity.RegisterNew(ctx, input);  // 1
if (entity is null) return null;              // 2
bool ok = await _repo.RegisterNewAsync(       // 3
    ctx, entity, ct);
if (!ok) { AddError(ctx); return null; }      // 4
return entity;                                // 5

───────────────────────────────────────────────────────────────────────────────
LLM_TEMPLATE: Padrão de Operação de Modificação
───────────────────────────────────────────────────────────────────────────────

1. Carregar entidade do repositório
2. Verificar null (não encontrada)
3. Chamar método de negócio (retorna nova instância — imutabilidade)
4. Verificar null (falha de validação)
5. Retornar nova instância

var entity = await _repo.GetByIdAsync(ctx, id, ct);  // 1
if (entity is null) { AddError(ctx); return null; }  // 2
var updated = entity.ChangeName(ctx, input);          // 3
if (updated is null) { AddError(ctx); return null; }  // 4
return updated;                                        // 5

NOTA: O método ChangeName retorna NOVA instância — a original permanece
inalterada (imutabilidade). A persistência da mudança acontece na camada
Application (UseCase com UnitOfWork).

═══════════════════════════════════════════════════════════════════════════════
*/

public sealed class SimpleAggregateRootService : ISimpleAggregateRootService
{
    private const string RegistrationFailedMessageCode = "SimpleAggregateRootService.RegistrationFailed";
    private const string NotFoundMessageCode = "SimpleAggregateRootService.NotFound";
    private const string ChangeNameFailedMessageCode = "SimpleAggregateRootService.ChangeNameFailed";

    private readonly ISimpleAggregateRootRepository _repository;

    public SimpleAggregateRootService(
        ISimpleAggregateRootRepository repository
    )
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<SimpleAggregateRoot?> RegisterAsync(
        ExecutionContext executionContext,
        string firstName,
        string lastName,
        BirthDate birthDate,
        CancellationToken cancellationToken
    )
    {
        var input = new RegisterNewInput(firstName, lastName, birthDate);
        var aggregateRoot = SimpleAggregateRoot.RegisterNew(executionContext, input);

        if (aggregateRoot is null)
            return null;

        bool persisted = await _repository.RegisterNewAsync(
            executionContext,
            aggregateRoot,
            cancellationToken);

        if (!persisted)
        {
            if (!executionContext.HasErrorMessages)
                executionContext.AddErrorMessage(code: RegistrationFailedMessageCode);

            return null;
        }

        return aggregateRoot;
    }

    public async Task<SimpleAggregateRoot?> ChangeNameAsync(
        ExecutionContext executionContext,
        Id id,
        string firstName,
        string lastName,
        CancellationToken cancellationToken
    )
    {
        var aggregateRoot = await _repository.GetByIdAsync(
            executionContext,
            id,
            cancellationToken);

        if (aggregateRoot is null)
        {
            executionContext.AddErrorMessage(code: NotFoundMessageCode);
            return null;
        }

        var input = new ChangeNameInput(firstName, lastName);
        var updated = aggregateRoot.ChangeName(executionContext, input);

        if (updated is null)
        {
            if (!executionContext.HasErrorMessages)
                executionContext.AddErrorMessage(code: ChangeNameFailedMessageCode);

            return null;
        }

        return updated;
    }
}
