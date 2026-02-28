using Bedrock.BuildingBlocks.Core.BirthDates;
using Bedrock.BuildingBlocks.Core.Ids;
using Templates.Domain.Entities.SimpleAggregateRoots;

namespace Templates.Domain.Services.Interfaces;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: Interface de Domain Service
═══════════════════════════════════════════════════════════════════════════════

Domain Services orquestram operações que envolvem múltiplos conceitos
(entidades, repositórios, value objects) e não pertencem a uma única entidade.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Retorno Nullable Para Operações Que Podem Falhar
───────────────────────────────────────────────────────────────────────────────

Métodos de service retornam Task<T?> — null indica falha.
Detalhes do erro ficam no ExecutionContext.Messages.

✅ Task<SimpleAggregateRoot?> RegisterAsync(ctx, ...);
❌ Task<SimpleAggregateRoot> RegisterAsync(ctx, ...); // exceção em falha

RAZÃO: O consumidor verifica null e consulta ExecutionContext para mensagens.
Exceções são reservadas para erros técnicos inesperados, não para falhas
de validação ou regra de negócio.

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Assinatura Padrão dos Métodos
───────────────────────────────────────────────────────────────────────────────

✅ Primeiro parâmetro: ExecutionContext (correlação, tenant, audit)
✅ Último parâmetro: CancellationToken
✅ Parâmetros de negócio no meio, tipados (não use string para tudo)

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Quando Criar um Domain Service vs Lógica na Entidade
───────────────────────────────────────────────────────────────────────────────

Lógica que depende APENAS do estado da entidade → método na entidade.
Lógica que precisa de repositório ou cruza agregados → Domain Service.

✅ SimpleAggregateRoot.ChangeName() — validação interna, sem I/O
✅ SimpleAggregateRootService.RegisterAsync() — cria entidade + persiste
❌ SimpleAggregateRoot.SaveAsync() — entidade NÃO conhece persistência

═══════════════════════════════════════════════════════════════════════════════
*/

public interface ISimpleAggregateRootService
{
    Task<SimpleAggregateRoot?> RegisterAsync(
        ExecutionContext executionContext,
        string firstName,
        string lastName,
        BirthDate birthDate,
        CancellationToken cancellationToken);

    Task<SimpleAggregateRoot?> ChangeNameAsync(
        ExecutionContext executionContext,
        Id id,
        string firstName,
        string lastName,
        CancellationToken cancellationToken);
}
