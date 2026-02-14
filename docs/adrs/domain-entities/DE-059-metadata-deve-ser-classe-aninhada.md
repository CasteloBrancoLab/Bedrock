# DE-059: Metadados Devem Ser Classe Aninhada da Entidade

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma **receita de bolo** com suas **informações nutricionais**:

**Opção A - Informações em folha separada**:
A receita está em uma página, mas os valores nutricionais (calorias, gorduras, etc.) estão em outro caderno. Para entender o bolo completo, você precisa consultar dois lugares.

**Opção B - Informações na mesma página**:
A receita e as informações nutricionais estão juntas na mesma página. Tudo sobre o bolo está em um único lugar.

Em código, uma classe de metadados separada é a "folha separada" - ela pode até ficar perdida ou dessincronizada. Uma classe aninhada é a "mesma página" - os metadados são parte intrínseca da entidade.

---

### O Problema Técnico

Metadados de entidade (limites de validação, flags de obrigatoriedade) podem ser definidos de duas formas:

```csharp
// Opção A: Classe separada no mesmo namespace (ERRADO)
// Arquivo: Users/User.cs
public sealed class User : EntityBase<User> { ... }

// Arquivo: Users/UserMetadata.cs
public static class UserMetadata
{
    public static int UsernameMaxLength { get; private set; } = 255;
    public static bool UsernameIsRequired { get; private set; } = true;
}

// Opção B: Classe aninhada dentro da entidade (CORRETO)
// Arquivo: Users/User.cs
public sealed class User : EntityBase<User>
{
    // ... propriedades, métodos ...

    public static class UserMetadata
    {
        public static int UsernameMaxLength { get; private set; } = 255;
        public static bool UsernameIsRequired { get; private set; } = true;
    }
}
```

A classe separada cria **fragmentação de responsabilidade**: os metadados pertencem à entidade, mas estão fisicamente e logicamente separados dela.

## A Decisão

### Nossa Abordagem

A classe de metadados **DEVE** ser uma classe estática aninhada dentro da entidade, seguindo o padrão `{EntityName}Metadata`:

```csharp
public sealed class SimpleAggregateRoot : EntityBase<SimpleAggregateRoot>,
    IAggregateRoot,
    ISimpleAggregateRoot
{
    // Properties
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    // ... constructors, methods, validation ...

    // Metadata - SEMPRE no final da classe, aninhada
    public static class SimpleAggregateRootMetadata
    {
        private static readonly Lock _lockObject = new();

        public static readonly string FirstNamePropertyName = nameof(FirstName);
        public static bool FirstNameIsRequired { get; private set; } = true;
        public static int FirstNameMinLength { get; private set; } = 1;
        public static int FirstNameMaxLength { get; private set; } = 255;

        public static readonly string LastNamePropertyName = nameof(LastName);
        public static bool LastNameIsRequired { get; private set; } = true;
        public static int LastNameMinLength { get; private set; } = 1;
        public static int LastNameMaxLength { get; private set; } = 255;

        // Change methods...
    }
}
```

### Por Que Funciona Melhor

1. **Coesão**: Metadados pertencem à entidade - aninhamento expressa essa relação
2. **Encapsulamento**: A classe aninhada pode acessar membros privados se necessário
3. **Navegabilidade**: Um único arquivo para entender toda a entidade
4. **Namespace limpo**: Não polui o namespace com tipos auxiliares
5. **Convenção de nomenclatura**: `nameof(FirstName)` funciona porque o contexto é a entidade

### Benefícios

1. **Localidade de referência**:
   - Tudo sobre a entidade em um arquivo
   - Sem navegação entre arquivos para entender constraints

2. **Refactoring seguro**:
   - Renomear a entidade ou suas propriedades atualiza automaticamente o contexto
   - IDE pode navegar diretamente para a metadata via nested type

3. **Consistência com o template**:
   - O template `SimpleAggregateRoot.cs` define o padrão canonical
   - Todas as entidades seguem a mesma estrutura

4. **Prevenção de erros**:
   - Impossível ter metadata órfã (sem entidade correspondente)
   - Renomear a entidade quebra a compilação se metadata estiver inconsistente

### Trade-offs (Com Perspectiva)

- **Arquivo maior**: A entidade fica mais longa com metadata aninhada

Na prática isso raramente é problema porque:
- IDE permite collapse/fold de regiões
- Metadata fica no **final** da classe, não interfere na leitura do código principal
- Um arquivo coeso é melhor que dois arquivos fragmentados

### Trade-offs Frequentemente Superestimados

**"Arquivo separado é mais organizado"**

Na verdade, é o oposto. Classe separada:
- Adiciona mais um arquivo para manter
- Pode ficar dessincronizada com a entidade
- Requer mais imports/usings nos testes
- Polui o namespace com tipo auxiliar

**"A entidade fica muito grande"**

A metadata é a última seção da classe. Ela não interfere na leitura das propriedades, constructors, métodos de negócio ou validação. Na prática, ao ler a entidade, a metadata fica "fora do caminho".

## Fundamentação Teórica

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008):

> "Classes should be small... But we measure the size of a class by counting responsibilities, not lines."
>
> *Classes devem ser pequenas... Mas medimos o tamanho de uma classe contando responsabilidades, não linhas.*

Metadata de validação **é** uma responsabilidade da entidade - definir seus próprios limites e constraints. Separá-la viola o Single Responsibility Principle porque fragmenta uma responsabilidade coesa.

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003):

> "An Entity's identity and lifecycle are its most fundamental characteristics."
>
> *A identidade e ciclo de vida de uma Entidade são suas características mais fundamentais.*

Os constraints de validação (maxLength, isRequired) são parte da **definição** da entidade. Separá-los é como separar o CPF da pessoa - tecnicamente possível, mas semanticamente errado.

### Princípio da Coesão

Classes aninhadas em C# expressam uma relação "belongs-to" forte:

> "Nested types are useful when the class is only meaningful in the context of its containing type."
>
> *Tipos aninhados são úteis quando a classe só faz sentido no contexto do tipo que a contém.*
> — C# Design Guidelines

`UserMetadata` só faz sentido no contexto de `User`. Não existe `UserMetadata` independente.

## Antipadrões Documentados

### Antipadrão 1: Metadata em Arquivo Separado

```csharp
// ? Users/UserMetadata.cs - arquivo separado
public static class UserMetadata
{
    public static int UsernameMaxLength { get; private set; } = 255;
}

// ? Users/User.cs - referencia metadata externa
public sealed class User : EntityBase<User>
{
    // Usa UserMetadata.UsernameMaxLength em validação
}
```

### Antipadrão 2: Metadata em Namespace Diferente

```csharp
// ? Metadata/UserMetadata.cs - namespace diferente!
namespace ShopDemo.Auth.Domain.Entities.Users.Metadata;
public static class UserMetadata { ... }
```

### Antipadrão 3: Metadata com Nome Incorreto

```csharp
public sealed class User : EntityBase<User>
{
    // ? Nome errado - deve ser UserMetadata
    public static class UserConstraints { ... }
    public static class UserValidation { ... }
    public static class UserConfig { ... }
}
```

## Decisões Relacionadas

- [DE-012](./DE-012-metadados-estaticos-vs-data-annotations.md) - Por que usar propriedades estáticas
- [DE-013](./DE-013-nomenclatura-de-metadados.md) - Convenção de nomenclatura `{PropertyName}{ConstraintType}`
- [DE-014](./DE-014-inicializacao-inline-de-metadados.md) - Inicialização inline
- [DE-015](./DE-015-customizacao-de-metadados-apenas-no-startup.md) - Customização apenas no startup

## Leitura Recomendada

- [C# Nested Types Design Guidelines](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/nested-types)
- [Clean Code - Chapter 10: Classes](https://www.oreilly.com/library/view/clean-code-a/9780136083238/)
- [Domain-Driven Design - Chapter 5: Entities](https://www.domainlanguage.com/ddd/)

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Define o padrão de classe aninhada de metadados no template canonical |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../src/Templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - Template canonical com metadata aninhada
- [CompositeAggregateRoot.cs](../../../src/Templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - Composite AR com metadata aninhada
- [CompositeChildEntity.cs](../../../src/Templates/Domain.Entities/CompositeAggregateRoots/CompositeChildEntity.cs) - Child entity com metadata aninhada
