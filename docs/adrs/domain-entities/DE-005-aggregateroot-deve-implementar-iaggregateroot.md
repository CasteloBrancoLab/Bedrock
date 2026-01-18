# DE-005: AggregateRoot Deve Implementar IAggregateRoot

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um prédio corporativo com diferentes níveis de acesso. Há funcionários regulares, gerentes e diretores. Todos são "pessoas que trabalham na empresa", mas diretores têm acesso a áreas restritas como a sala de reuniões do conselho.

Para controlar esse acesso, o prédio usa crachás. Todo funcionário tem um crachá (é uma "pessoa"), mas diretores têm um crachá especial com uma marca distintiva que permite acesso às áreas restritas.

Na programação, temos situação análoga: todas as entidades de domínio são "entidades" (`IEntity`), mas Aggregate Roots são entidades especiais que servem como ponto de entrada para agregados inteiros. Precisamos de uma forma de identificá-los e garantir que apenas Aggregate Roots sejam usados onde Aggregate Roots são esperados.

### O Problema Técnico

Sem uma interface específica para Aggregate Roots:

1. **Falta de type safety**: Qualquer entidade pode ser passada onde um Aggregate Root é esperado
2. **Identificação difícil**: Código precisa verificar por convenção ou reflection
3. **Contratos implícitos**: A intenção de uma classe ser Aggregate Root fica apenas em documentação

```csharp
// ❌ Sem interface específica - qualquer entidade serve
public interface IRepository<TEntity> where TEntity : IEntity
{
    TEntity GetById(Guid id);
}

// Problema: posso criar repository para entidades internas do agregado
var orderItemRepository = new Repository<OrderItem>();  // Compila, mas viola DDD!
```

## Como Normalmente É Feito

### Abordagem Tradicional

A maioria dos projetos não diferencia Aggregate Roots de outras entidades no sistema de tipos:

```csharp
// Abordagem comum - sem distinção no tipo
public class Order : Entity
{
    // É um Aggregate Root, mas o sistema de tipos não sabe disso
}

public class OrderItem : Entity
{
    // É uma entidade interna, mas o sistema de tipos não diferencia
}

// Repositórios aceitam qualquer entidade
public class Repository<T> where T : Entity
{
    public T GetById(Guid id) { /* ... */ }
}

// Problema: nada impede isso
var itemRepo = new Repository<OrderItem>();  // Viola DDD!
```

### Por Que Não Funciona Bem

1. **Violação de agregados**: Repositórios podem ser criados para entidades internas
2. **APIs inconsistentes**: Serviços de domínio não conseguem exigir Aggregate Roots
3. **Documentação vs código**: A distinção existe apenas em comentários/documentação
4. **Erros em runtime**: Problemas só são descobertos em tempo de execução

## A Decisão

### Nossa Abordagem

Todo Aggregate Root DEVE implementar `IAggregateRoot`:

```csharp
// Interface marker que identifica Aggregate Roots
public interface IAggregateRoot : IEntity
{
    // Interface marker - sem membros adicionais
    // A semântica está na hierarquia: IAggregateRoot é um IEntity especial
}

// Aggregate Root implementa explicitamente
public sealed class SimpleAggregateRoot
    : EntityBase<SimpleAggregateRoot>
    , IAggregateRoot
{
    // ...
}
```

### Por Que Funciona Melhor

1. **Type Safety em tempo de compilação**:
```csharp
// ✅ Repository só aceita Aggregate Roots
public interface IRepository<TAggregateRoot>
    where TAggregateRoot : IAggregateRoot
{
    TAggregateRoot GetById(Guid id);
}

// ❌ Não compila - OrderItem não é IAggregateRoot
var itemRepo = new Repository<OrderItem>();  // Erro de compilação!
```

2. **Identificação programática**:
```csharp
// Verificar se tipo é Aggregate Root
bool isAggregateRoot = typeof(IAggregateRoot).IsAssignableFrom(entityType);

// Descobrir todos os Aggregate Roots via reflection
var aggregateRoots = assembly.GetTypes()
    .Where(t => typeof(IAggregateRoot).IsAssignableFrom(t) && !t.IsInterface);
```

3. **Contratos explícitos em serviços**:
```csharp
// Serviço de domínio que exige Aggregate Root
public class DomainEventPublisher
{
    public void Publish<TAggregateRoot>(TAggregateRoot aggregateRoot)
        where TAggregateRoot : IAggregateRoot
    {
        // Apenas Aggregate Roots podem publicar eventos de domínio
    }
}
```

## Consequências

### Benefícios

- **Type safety**: Compilador garante que apenas Aggregate Roots são usados onde esperados
- **Design by Contract**: Interface documenta explicitamente a intenção da classe
- **Polimorfismo controlado**: Código genérico pode trabalhar com qualquer Aggregate Root
- **Descoberta automática**: Ferramentas podem encontrar Aggregate Roots via reflection
- **Consistência arquitetural**: Força desenvolvedores a declarar explicitamente Aggregate Roots

### Trade-offs (Com Perspectiva)

- **Código adicional**: Cada Aggregate Root precisa declarar a interface
- **Disciplina requerida**: Desenvolvedores devem lembrar de implementar a interface

### Trade-offs Frequentemente Superestimados

**"É só uma interface marker, não adiciona funcionalidade"**

Interfaces marker são uma forma legítima de metadados em tempo de compilação. Diferente de attributes:
- São verificadas pelo compilador (constraints `where T : IAggregateRoot`)
- Não requerem reflection para verificação básica
- Participam do sistema de tipos

**"Posso usar convenção de nomenclatura"**

Convenções (`*AggregateRoot`, `*Root`) são frágeis:
- Não são verificadas em tempo de compilação
- Podem ser esquecidas ou aplicadas inconsistentemente
- Requerem string matching ou regex

## Fundamentação Teórica

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) define claramente o papel especial de Aggregate Roots:

> "Choose one ENTITY to be the root of each AGGREGATE, and control all access to the objects inside the boundary through the root."
>
> *Escolha uma ENTITY para ser a raiz de cada AGGREGATE, e controle todo o acesso aos objetos dentro da fronteira através da raiz.*

A interface `IAggregateRoot` é a implementação técnica dessa distinção conceitual.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) reforça:

> "Only Aggregate Roots can be obtained directly with database queries. [...] References to an Aggregate Root should be held only temporarily, with the reference being abandoned after the unit of work completes."
>
> *Apenas Aggregate Roots podem ser obtidos diretamente com queries de banco de dados. [...] Referências a um Aggregate Root devem ser mantidas apenas temporariamente.*

O constraint `where T : IAggregateRoot` em repositórios implementa exatamente essa regra.

### O Que o Clean Architecture Diz

Clean Architecture enfatiza **boundaries** claros entre camadas. A interface `IAggregateRoot` define uma boundary clara entre:
- Entidades que são pontos de entrada (Aggregate Roots)
- Entidades internas que só podem ser acessadas via seu Aggregate Root

### Padrões de Design Relacionados

**Marker Interface Pattern** - Uma interface sem membros que serve para identificar/categorizar tipos. Exemplos clássicos incluem `Serializable` em Java e `ICloneable` em .NET.

**Design by Contract** - A interface `IAggregateRoot` é um contrato que diz: "esta classe é um Aggregate Root e pode ser usada como tal".

## Antipadrões Documentados

### Usar Atributos ao Invés de Interfaces

```csharp
// ❌ Atributo não participa do sistema de tipos
[AggregateRoot]
public class Order : Entity { }

// Problema: não funciona com constraints genéricos
public interface IRepository<T> where T : ??? // Não existe "where T has [AggregateRoot]"
{
}
```

### Depender Apenas de Convenção de Nomes

```csharp
// ❌ Convenção pode ser esquecida ou ignorada
public class OrderRoot : Entity { }  // "Root" no nome, mas sem interface
public class CustomerAggregate : Entity { }  // Inconsistente com "Root"
```

### Não Implementar IAggregateRoot em Aggregate Roots

```csharp
// ❌ Herda de EntityBase mas não implementa IAggregateRoot
public sealed class Order : EntityBase<Order>
{
    // Funciona, mas perde type safety nos repositories
}
```

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Qual a diferença entre marker interfaces e atributos em C#?"
- "Como usar generic constraints para garantir padrões arquiteturais?"
- "Por que DDD distingue Aggregate Roots de outras entidades?"
- "Como implementar descoberta automática de Aggregate Roots para migração de banco?"

### Leitura Recomendada

- [Domain-Driven Design - Aggregates](https://martinfowler.com/bliki/DDD_Aggregate.html)
- [Effective Aggregate Design](https://www.dddcommunity.org/library/vernon_2011/)
- [Marker Interface Pattern](https://en.wikipedia.org/wiki/Marker_interface_pattern)

## Decisões Relacionadas

- [DE-001](./DE-001-entidades-devem-ser-sealed.md) - Entidades Devem Ser Sealed
- [DE-002](./DE-002-construtores-privados-com-factory-methods.md) - Construtores Privados com Factory Methods
- [DE-004](./DE-004-estado-invalido-nunca-existe-na-memoria.md) - Estado Inválido Nunca Existe na Memória
- [DE-027](./DE-027-entidades-nao-tem-dependencias-externas.md) - Entidades Não Têm Dependências Externas

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [IAggregateRoot](../../building-blocks/domain-entities/iaggregateroot.md) | Interface marker que identifica Aggregate Roots, estendendo IEntity |
| [IEntity](../../building-blocks/domain-entities/ientity.md) | Interface base que IAggregateRoot estende |
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Classe base que implementa IEntity, usada junto com IAggregateRoot |

## Referências no Código

- [IAggregateRoot.cs](../../../src/BuildingBlocks/Domain.Entities/Interfaces/IAggregateRoot.cs) - Definição da interface
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - declaração da classe implementando IAggregateRoot
