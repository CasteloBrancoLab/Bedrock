using Bedrock.BuildingBlocks.Core.Ids;

namespace Templates.Domain.Validators.Interfaces;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: Domain Validators - Validação Cross-Aggregate
═══════════════════════════════════════════════════════════════════════════════

Validators encapsulam regras de validação que PRECISAM de acesso a
repositórios ou cruzam fronteiras de agregados. Regras que dependem
apenas do estado interno da entidade ficam NA PRÓPRIA ENTIDADE.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Quando Usar Validator vs Validação na Entidade
───────────────────────────────────────────────────────────────────────────────

Validação na ENTIDADE (sem I/O):
✅ SimpleAggregateRoot.ValidateFirstName() — regra de formato/tamanho
✅ SimpleAggregateRoot.IsValid() — composição de validações locais

Validação no VALIDATOR (com I/O):
✅ ExistenceValidator.ValidateExistsAsync() — precisa consultar repositório
✅ RoleHierarchyValidator.ValidateNoCircularDependencyAsync() — cruza agregados

❌ Entity.ValidateUniqueEmailAsync() — entidade NÃO acessa repositório

RAZÃO: Entidades são puras (sem I/O). Validações com dependência externa
ficam em Validators, que recebem repositórios via DI.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Retorno bool + Mensagens no ExecutionContext
───────────────────────────────────────────────────────────────────────────────

✅ Task<bool> ValidateExistsAsync(ctx, id, ct); // true = válido
❌ Task<ValidationResult> ValidateExistsAsync(...); // DTO desnecessário

RAZÃO: O padrão do framework já usa ExecutionContext para mensagens.
Retornar bool é suficiente — o chamador verifica e o contexto tem os detalhes.

═══════════════════════════════════════════════════════════════════════════════
*/

public interface ISimpleAggregateRootExistenceValidator
{
    Task<bool> ValidateExistsAsync(
        ExecutionContext executionContext,
        Id id,
        CancellationToken cancellationToken);
}
