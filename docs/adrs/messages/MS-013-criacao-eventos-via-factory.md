# MS-013: Criação de Eventos Exclusivamente via Factory

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma montadora de carros. Se cada mecânico na linha de montagem soldasse peças à mão, sem gabaritos nem estação padronizada, cada carro sairia diferente — parafusos em posições distintas, fiação com cores variadas. A solução é centralizar a montagem em estações especializadas (factories). O mecânico pede à estação: "monte o painel de instrumentos" e recebe o componente pronto. Se a especificação mudar, muda-se apenas a estação — não todos os mecânicos.

### O Problema Técnico

Eventos de integração carregam Metadata, Input, OldState, NewState e Output. Construí-los inline no Use Case gera problemas:

- **Duplicação**: Cada Use Case repete a lógica de construir `MessageMetadata`, mapear entities para models, e montar o evento
- **Inconsistência**: Se a forma de criar Metadata mudar (ex: novo campo), todos os Use Cases precisam ser atualizados
- **Responsabilidade errada**: O Use Case orquestra; a construção de eventos é um detalhe técnico que não pertence à orquestração
- **Composição complexa**: Eventos self-contained (MS-010) precisam de múltiplas factories (Metadata, Model, Event) — coordenar essas chamadas no Use Case polui a lógica de negócio

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos projetos constroem eventos diretamente no Use Case:

```csharp
// Construção inline — poluído e duplicado
public class DeactivateUserUseCase
{
    public async Task Execute(...)
    {
        // ... lógica de negócio ...

        var metadata = new MessageMetadata(
            Guid.NewGuid(),
            timeProvider.GetUtcNow(),
            typeof(UserDeactivatedEvent).FullName!,
            executionContext.CorrelationId,
            // ... mais campos ...
        );
        var userModel = new UserModel(
            user.Id.Value,
            user.EntityInfo.TenantInfo.TenantCode.Value,
            // ... muitos mapeamentos ...
        );
        var evt = new UserDeactivatedEvent(metadata, input, oldModel, userModel, output);
    }
}
```

### Por Que Não Funciona Bem

- **Duplicação massiva**: Cada Use Case repete 20+ linhas de mapeamento
- **Fragilidade**: Mudar `MessageMetadata` exige alterar todos os Use Cases
- **Mistura de responsabilidades**: Use Case mistura orquestração com construção de DTOs
- **Difícil de testar**: Precisa verificar a construção do evento em cada Use Case

## A Decisão

### Nossa Abordagem

A criação de eventos é centralizada em factories no namespace `*.Application.Factories`. O Use Case faz **UMA** chamada à factory; factories chamam entre si lateralmente:

```csharp
// Use Case — UMA chamada à factory
public class DeactivateUserUseCase
{
    public async Task Execute(...)
    {
        // ... lógica de negócio ...

        var evt = AuthEventFactory.CreateUserDeactivated(
            executionContext, timeProvider,
            input, oldUser, user, deactivationOutput);

        await outbox.PublishAsync(evt);
    }
}

// Factory — coordena a construção
public static class AuthEventFactory
{
    public static UserDeactivatedEvent CreateUserDeactivated(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        DeactivateUserInputModel input,
        User oldUser,
        User newUser,
        UserDeactivationOutput output)
    {
        // Factories chamam entre si lateralmente
        var metadata = AuthMessageMetadataFactory.Create(executionContext, timeProvider);
        var oldModel = UserModelFactory.FromEntity(oldUser, executionContext);
        var newModel = UserModelFactory.FromEntity(newUser, executionContext);

        return new UserDeactivatedEvent(metadata, input, oldModel, newModel,
            new DeactivationOutputModel(
                output.RevokedRefreshTokenCount,
                output.RevokedServiceClientCount,
                output.RevokedApiKeyCount));
    }
}
```

O que é proibido:

```csharp
// PROIBIDO em Application (fora de Factories): instanciar EventBase-derived
public class SomeUseCase
{
    public async Task Execute(...)
    {
        var evt = new UserDeactivatedEvent(...); // ← violação MS-013
    }
}
```

### Por Que Funciona Melhor

1. **Single Responsibility**: Use Case orquestra; Factory constrói
2. **DRY**: Lógica de mapeamento centralizada — muda em um lugar
3. **Testabilidade**: Factory testável isoladamente; Use Case testa apenas orquestração
4. **Composição lateral**: Factories chamam entre si sem depender do Use Case

## Consequências

### Benefícios

- **Manutenibilidade**: Mudança em Metadata ou Model afeta apenas a factory correspondente
- **Consistência**: Todos os eventos são construídos da mesma forma
- **Simplicidade no Use Case**: Uma linha para criar o evento, uma linha para publicar
- **Extensibilidade**: Novo evento = novo método na factory, sem tocar em Use Cases existentes

### Trade-offs (Com Perspectiva)

- **Indireção**: Use Case não vê como o evento é construído
  - Isso é desejável — o Use Case não deve conhecer os detalhes de construção. A factory encapsula essa complexidade
- **Mais arquivos**: Uma factory por bounded context
  - O custo de um arquivo é insignificante comparado à duplicação que elimina
- **Rigidez**: Toda criação de evento deve passar pela factory
  - A regra MS-013 automatiza essa validação — não depende de code review manual

## Fundamentação Teórica

### Padrões de Design Relacionados

**Factory Method / Static Factory**: Centraliza a criação de objetos complexos. A factory conhece os detalhes de construção; o cliente (Use Case) conhece apenas a interface.

**Façade**: A factory atua como façade sobre múltiplas factories internas (Metadata, Model) — o Use Case faz uma chamada e recebe o evento pronto.

> "Encapsulate what varies."
>
> *Encapsule o que varia.*
>
> — Gang of Four

### O Que o Clean Architecture Diz

No Clean Architecture, Use Cases contêm regras de aplicação (orquestração). A construção de DTOs de infraestrutura (eventos com Metadata) é um detalhe que pertence à camada de adaptadores. A factory faz essa ponte entre o domínio e a infraestrutura de mensagens.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Qual a diferença entre Factory Method e Static Factory no contexto de eventos?"
- "Como o padrão Façade se aplica à criação de eventos de integração?"
- "Por que a construção de DTOs não pertence ao Use Case?"

### Leitura Recomendada

- [Factory Method - Refactoring Guru](https://refactoring.guru/design-patterns/factory-method)
- [Façade Pattern - GoF](https://refactoring.guru/design-patterns/facade)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Messages | Define EventBase, base dos eventos criados pelas factories |
| Bedrock.BuildingBlocks.Outbox | Consome os eventos criados pelas factories |

## Referências no Código

- [AuthEventFactory.cs](../../../src/ShopDemo/Auth/Application/Factories/AuthEventFactory.cs) - factory central de eventos
- [AuthMessageMetadataFactory.cs](../../../src/ShopDemo/Auth/Application/Factories/AuthMessageMetadataFactory.cs) - factory de Metadata (chamada lateralmente)
- [UserModelFactory.cs](../../../src/ShopDemo/Auth/Application/Factories/UserModelFactory.cs) - factory de Model (chamada lateralmente)
