# DE-060: Interface de Domínio Deve Declarar IAggregateRoot

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma empresa onde o organograma diz que João é "funcionário", mas na prática ele é o CEO e toma decisões de diretoria. Quando alguém consulta o organograma para saber quem pode aprovar orçamentos, João não aparece - porque o organograma o classifica apenas como "funcionário".

O problema não é que João exerça a função errada, mas que o **documento oficial** (organograma) descreve seu papel num nível errado. Quem depende do organograma toma decisões incorretas.

Na programação, a interface de domínio (`IUser`, `IOrder`) é o "organograma" da entidade. Se ela declara `IEntity` quando a entidade é na verdade um Aggregate Root, todo código que depende da interface não sabe que está lidando com um Aggregate Root.

### O Problema Técnico

Quando a interface de domínio herda de `IEntity` mas a classe concreta implementa `IAggregateRoot`, cria-se uma **assimetria entre contrato e implementação**:

```csharp
// ❌ Interface declara nível errado
public interface IUser : IEntity  // "Sou apenas uma entidade"
{
    string Username { get; }
}

// Classe concreta eleva o nível
public sealed class User
    : EntityBase<User>
    , IAggregateRoot   // "Na verdade sou um Aggregate Root"
    , IUser
{
}
```

Problemas concretos:

1. **Repositórios não aceitam a interface**: `IRepository<T> where T : IAggregateRoot` não aceita `IUser` porque `IUser : IEntity`, não `IAggregateRoot`
2. **Serviços de domínio forçam downcast**: Código que recebe `IUser` precisa fazer cast para `IAggregateRoot`
3. **Incentivo ao erro incremental**: Ao adicionar um repository ou serviço de domínio, o code agent modifica a **classe** para satisfazer o constraint, em vez de corrigir a **interface**
4. **Contrato mentiroso**: Consumidores de `IUser` não sabem que estão operando sobre um Aggregate Root

## Como Normalmente É Feito

### Abordagem Tradicional

A maioria dos projetos define interfaces de domínio sem considerar a hierarquia DDD:

```csharp
// Interface genérica - nível de abstração indefinido
public interface IUser
{
    string Username { get; }
    string Email { get; }
}

// Classe decide sozinha o que ela é
public class User : Entity, IAggregateRoot, IUser
{
    // A interface não participa da decisão
}
```

### Por Que Não Funciona Bem

1. **Decisão diferida para a classe**: A interface não expressa se a entidade é raiz de agregado ou entidade interna
2. **Erro incremental silencioso**: Quando um novo requisito exige `IAggregateRoot` (ex: repository), o caminho de menor resistência é adicionar na classe, não na interface
3. **Dependência invertida quebrada**: Camadas superiores dependem da interface, mas a informação arquitetural está apenas na classe concreta
4. **Code agents erram sistematicamente**: LLMs priorizam compilação rápida - ao encontrar um constraint `where T : IAggregateRoot`, adicionam na classe porque é o caminho mais curto para compilar

## A Decisão

### Nossa Abordagem

A interface de domínio DEVE herdar de `IAggregateRoot` quando a entidade é um Aggregate Root:

```csharp
// ✅ Interface declara o nível correto
public interface IUser
    : IAggregateRoot  // Contrato explícito: IUser é um Aggregate Root
{
    string Username { get; }
    string Email { get; }
    PasswordHash PasswordHash { get; }
}

// Classe implementa a interface (que já inclui IAggregateRoot)
public sealed class User
    : EntityBase<User>
    , IUser            // IAggregateRoot já vem via IUser
{
    // ...
}
```

Da mesma forma, para entidades internas de um agregado:

```csharp
// ✅ Entidade interna herda de IEntity (não IAggregateRoot)
public interface IOrderItem : IEntity
{
    decimal Price { get; }
    int Quantity { get; }
}
```

### Regra

> **A interface de domínio da entidade DEVE herdar da interface de infraestrutura correspondente ao seu papel no agregado:**
> - Aggregate Root → `IAggregateRoot`
> - Entidade interna do agregado → `IEntity`

### Por Que Funciona Melhor

1. **Contrato honesto**: A interface expressa exatamente o papel da entidade no domínio
2. **Type safety propagado**: Código que recebe `IUser` já sabe que é `IAggregateRoot`
3. **Repositories e serviços funcionam naturalmente**:

```csharp
// ✅ Funciona porque IUser : IAggregateRoot
public interface IUserRepository : IRepository<IUser>
{
    Task<IUser?> FindByEmail(Email email);
}
```

4. **Previne erro incremental**: Não há necessidade de modificar a classe quando um constraint exige `IAggregateRoot` - a interface já satisfaz

## Consequências

### Benefícios

- **Decisão arquitetural na interface**: O papel da entidade fica no contrato, visível para todos os consumidores
- **Eliminação de assimetria**: Interface e classe concordam sobre o nível de abstração
- **Prevenção de erro em code agents**: LLMs não precisam "consertar" a classe porque a interface já tem a informação correta
- **Dependency Inversion correto**: Camadas superiores dependem da interface e recebem toda a informação de tipo necessária

### Trade-offs (Com Perspectiva)

- **Decisão antecipada**: É preciso decidir se a entidade é Aggregate Root no momento de criar a interface. Na prática, essa decisão já é obrigatória em DDD ao modelar agregados - a ADR apenas exige que ela seja **registrada na interface** ao invés de apenas na classe.

## Fundamentação Teórica

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003):

> "The root is the only member of the AGGREGATE that outside objects are allowed to hold references to."
>
> *A raiz é o único membro do AGGREGATE ao qual objetos externos podem manter referências.*

Se objetos externos referenciam a entidade via interface, e a interface não declara que é um Aggregate Root, perde-se essa distinção fundamental. A interface é a **referência pública** da entidade - ela deve carregar a classificação correta.

### O Que o Clean Architecture Diz

Robert C. Martin em "Clean Architecture" (2017) enfatiza o **Dependency Inversion Principle**:

> "Depend on abstractions, not on concretions."
>
> *Dependa de abstrações, não de implementações concretas.*

Se a abstração (`IUser`) é mais pobre que a implementação (`User : IAggregateRoot`), as camadas superiores perdem informação ao depender da abstração. A interface deve ser **tão expressiva quanto necessário** para que os consumidores não precisem conhecer a classe concreta.

### Padrões de Design Relacionados

**Interface Segregation Principle (ISP)** - Interfaces devem ser específicas para seus consumidores. Repositories e domain services são consumidores que **precisam** saber que estão lidando com Aggregate Roots. Se a interface omite essa informação, estamos violando ISP ao forçar esses consumidores a lidar com um tipo genérico demais.

**Liskov Substitution Principle (LSP)** - Se `User` implementa `IAggregateRoot` mas `IUser` não herda de `IAggregateRoot`, então `IUser` e `User` têm contratos diferentes. Código que aceita `IAggregateRoot` não pode aceitar `IUser`, quebrando a substituibilidade.

## Antipadrões Documentados

### Interface Herda de IEntity, Classe Implementa IAggregateRoot

```csharp
// ❌ ANTIPADRÃO: Assimetria interface/classe
public interface IUser : IEntity { }

public sealed class User
    : EntityBase<User>
    , IAggregateRoot   // Elevação na classe
    , IUser
{ }

// Consequência: IUserRepository não compila
public interface IUserRepository : IRepository<IUser> { }
//                                              ^^^^
// Erro: IUser não satisfaz constraint 'IAggregateRoot'
```

### Interface Sem Herança de Tipo Base

```csharp
// ❌ ANTIPADRÃO: Interface "solta" sem classificação
public interface IUser
{
    string Username { get; }
}

// Problema: nenhuma informação sobre o papel da entidade
// Code agent pode fazer qualquer coisa com IUser
```

### Classe Implementa IAggregateRoot Diretamente para "Resolver" Compilação

```csharp
// ❌ ANTIPADRÃO: Solução de menor resistência
// Code agent encontra: IRepository<T> where T : IAggregateRoot
// Code agent adiciona IAggregateRoot na classe em vez de corrigir a interface

// Antes (interface correta mas incompleta):
public interface IUser : IEntity { }

// Depois (code agent "conserta" na classe):
public sealed class User : EntityBase<User>, IAggregateRoot, IUser { }
//                                           ^^^^^^^^^^^^^^
// O fix correto seria: public interface IUser : IAggregateRoot { }
```

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Qual a diferença entre declarar IAggregateRoot na interface vs na classe concreta?"
- "Como o Dependency Inversion Principle se aplica a interfaces de domínio em DDD?"
- "Por que code agents tendem a modificar classes concretas em vez de interfaces?"
- "Como a hierarquia de interfaces afeta generic constraints em repositórios?"

### Leitura Recomendada

- [Domain-Driven Design - Aggregates](https://martinfowler.com/bliki/DDD_Aggregate.html)
- [Effective Aggregate Design](https://www.dddcommunity.org/library/vernon_2011/)
- [Interface Segregation Principle](https://en.wikipedia.org/wiki/Interface_segregation_principle)

## Decisões Relacionadas

- [DE-005](./DE-005-aggregateroot-deve-implementar-iaggregateroot.md) - AggregateRoot Deve Implementar IAggregateRoot (complementar: DE-005 trata da classe, DE-060 trata da interface)
- [DE-001](./DE-001-entidades-devem-ser-sealed.md) - Entidades Devem Ser Sealed
- [DE-027](./DE-027-entidades-nao-tem-dependencias-externas.md) - Entidades Não Têm Dependências Externas

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| [IAggregateRoot](../../building-blocks/domain-entities/iaggregateroot.md) | Interface marker que deve ser herdada pela interface de dominio, nao apenas implementada pela classe |
| [IEntity](../../building-blocks/domain-entities/ientity.md) | Interface base para entidades internas do agregado (nao Aggregate Roots) |

## Referências no Código

- [IAggregateRoot.cs](../../../src/BuildingBlocks/Domain.Entities/Interfaces/IAggregateRoot.cs) - Definicao da interface
- [IUser.cs](../../../samples/ShopDemo/Auth/Domain.Entities/Users/Interfaces/IUser.cs) - Exemplo correto: interface herdando IAggregateRoot
- [User.cs](../../../samples/ShopDemo/Auth/Domain.Entities/Users/User.cs) - Classe que implementa IUser (IAggregateRoot propagado via interface)
