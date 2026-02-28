using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Templates.Domain.Entities.SimpleAggregateRoots;

namespace Templates.Domain.Repositories.Interfaces;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: Repository Interface no Domínio - Dependency Inversion Principle
═══════════════════════════════════════════════════════════════════════════════

A interface do repositório é definida no DOMÍNIO, não na infraestrutura.
A infraestrutura implementa. Isso é o Dependency Inversion Principle (DIP).

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Herdar de IRepository<TAggregateRoot>
───────────────────────────────────────────────────────────────────────────────

Toda interface de repositório DEVE herdar de IRepository<TAggregateRoot>.
A interface base já fornece:
✅ GetByIdAsync — carrega agregado completo
✅ ExistsAsync — verifica existência sem carregar
✅ EnumerateAllAsync — listagem paginada com handler
✅ EnumerateModifiedSinceAsync — sincronização temporal
✅ RegisterNewAsync — persistir novo agregado

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Métodos Customizados Para Queries de Negócio
───────────────────────────────────────────────────────────────────────────────

Se o domínio precisa de queries além das fornecidas por IRepository,
defina-as na interface específica:

✅ Task<User?> GetByEmailAsync(ctx, email, ct);
✅ Task<bool> ExistsByUsernameAsync(ctx, username, ct);
❌ Task<User?> FindByFieldAsync(string fieldName, object value); // genérico demais

RAZÃO: Nomes expressam intenção de negócio, não operação técnica.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Repositórios Apenas Para Aggregate Roots
───────────────────────────────────────────────────────────────────────────────

❌ IOrderItemRepository — OrderItem NÃO é Aggregate Root
✅ IOrderRepository — Order é Aggregate Root e persiste seus filhos

RAZÃO: Entidades filhas são persistidas junto com seu Aggregate Root.
Criar repositórios para filhos quebra a fronteira do agregado.

═══════════════════════════════════════════════════════════════════════════════
*/

public interface ISimpleAggregateRootRepository
    : IRepository<SimpleAggregateRoot>
{
}
