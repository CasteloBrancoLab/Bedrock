# DE-057: Metadata de Aggregate Roots Associadas - Apenas IsRequired

## Status
Aceita


## Contexto

### O Problema (Analogia)

Imagine um contrato de aluguel entre um inquilino e um imóvel. O contrato não define as regras de construção do imóvel (metragem mínima, número de cômodos, materiais). O imóvel já existe e foi construído seguindo suas próprias regras. O contrato apenas define: "é obrigatório ter um imóvel vinculado? Sim ou não."

Da mesma forma, quando uma Aggregate Root referencia outra Aggregate Root, ela não deve definir regras de validação sobre a estrutura interna da entidade referenciada. A entidade referenciada tem seu próprio ciclo de vida e suas próprias regras.

### O Problema Técnico

Quando uma Aggregate Root possui uma referência para outra Aggregate Root (associação), surge a dúvida sobre quais metadados de validação devem existir:

```csharp
// ❌ PROBLEMA: Tratar AR associada como propriedade simples
public static class PrimaryAggregateRootMetadata
{
    // Para strings, faz sentido:
    public static int NameMinLength { get; private set; } = 1;
    public static int NameMaxLength { get; private set; } = 255;

    // Mas para Aggregate Root associada, isto NÃO faz sentido:
    public static int ReferencedAggregateRootMinSomething { get; private set; } // ❌
    public static int ReferencedAggregateRootMaxSomething { get; private set; } // ❌
}
```

A Aggregate Root associada já tem suas próprias validações definidas em sua própria classe.

## Como Normalmente É Feito

### Abordagem Tradicional

A maioria dos projetos trata todas as propriedades da mesma forma, definindo metadados extensos mesmo para associações:

```csharp
public static class OrderMetadata
{
    // Propriedades simples
    public static int DescriptionMaxLength { get; } = 500;

    // Para Customer associado, alguns tentam:
    public static bool CustomerMustBeActive { get; } = true;        // ❌ Regra de negócio, não metadata
    public static int CustomerMinCreditScore { get; } = 500;        // ❌ Pertence ao Customer, não ao Order
    public static bool CustomerMustHaveValidEmail { get; } = true;  // ❌ Validação interna do Customer
}
```

### Por Que Não Funciona Bem

1. **Violação de responsabilidade**: Order não deveria saber detalhes internos de Customer
2. **Duplicação de regras**: Mesma validação em dois lugares (Customer e Order)
3. **Acoplamento indevido**: Mudança em Customer exige mudança em Order
4. **Confusão conceitual**: Mistura regras de associação com regras da entidade

## A Decisão

### Nossa Abordagem

Para associações entre Aggregate Roots, a ÚNICA validação no metadata é `IsRequired`:

```csharp
public sealed class PrimaryAggregateRoot
    : EntityBase<PrimaryAggregateRoot>,
    IAggregateRoot
{
    public static class PrimaryAggregateRootMetadata
    {
        // Propriedades simples: múltiplas validações
        public static bool QuantityIsRequired { get; private set; } = true;
        public static int QuantityMinValue { get; private set; } = 0;
        public static int QuantityMaxValue { get; private set; } = 1000;

        // ✅ Aggregate Root associada: APENAS IsRequired
        public static readonly string ReferencedAggregateRootPropertyName = nameof(ReferencedAggregateRoot);
        public static bool ReferencedAggregateRootIsRequired { get; private set; } = true;
        // Sem MinLength, MaxLength, MinValue, MaxValue, etc.
    }
}
```

### Onde Ficam as Outras Validações?

1. **Validações da entidade associada**: Na própria `ReferencedAggregateRoot` via `IsValid()`
2. **Regras de negócio contextuais**: Nos métodos `Validate*For*Internal`

```csharp
private static bool ValidateReferencedAggregateRootForRegisterNewInternal(
    ExecutionContext executionContext,
    ReferencedAggregateRoot? referencedAggregateRoot
)
{
    if (referencedAggregateRoot is null)
        return true; // IsRequired será validado no Set*

    // ✅ Validação da entidade em si (delega para ela mesma)
    bool isValid = referencedAggregateRoot.IsValid(executionContext);

    // ✅ Regras de negócio CONTEXTUAIS (específicas desta operação)
    // Ex: "Para RegisterNew, a AR associada não pode estar inativa"
    // Ex: "Para RegisterNew, SampleName não pode conflitar com X"

    return isValid;
}
```

### Por Que Funciona Melhor

1. **Responsabilidade única**: Cada AR valida a si mesma
2. **Zero duplicação**: Regras definidas em um único lugar
3. **Baixo acoplamento**: AR principal não conhece internals da associada
4. **Flexibilidade**: Regras contextuais nos métodos `Validate*For*Internal`

## Consequências

### Benefícios

- **Separação clara**: Metadata define obrigatoriedade, não estrutura
- **Ciclo de vida independente**: Cada AR evolui suas regras independentemente
- **Código limpo**: Metadata enxuto e focado
- **Extensibilidade**: Fácil adicionar regras contextuais por operação

### Trade-offs (Com Perspectiva)

- **Validação em dois lugares**: `IsRequired` no metadata + `IsValid()` na AR associada
  - *Perspectiva*: São responsabilidades diferentes - obrigatoriedade vs estrutura
- **Regras contextuais nos métodos**: Não ficam no metadata
  - *Perspectiva*: Regras de negócio específicas por operação pertencem aos métodos, não ao metadata estático

## Fundamentação Teórica

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) define claramente a separação entre agregados:

> "Each AGGREGATE has a root and a boundary. [...] Objects outside the AGGREGATE can hold references only to the root."
>
> *Cada AGREGADO tem uma raiz e uma fronteira. [...] Objetos fora do AGREGADO podem manter referências apenas para a raiz.*

A Aggregate Root associada é um agregado **separado** com suas próprias invariantes. A AR principal não deve tentar validar as invariantes internas de outro agregado.

### Single Responsibility Principle

Robert C. Martin define:

> "A class should have only one reason to change."
>
> *Uma classe deve ter apenas uma razão para mudar.*

Se `PrimaryAggregateRoot` definisse regras de validação para `ReferencedAggregateRoot`, ela teria múltiplas razões para mudar: suas próprias regras E as regras da associada.

### Tell, Don't Ask

Martin Fowler e Kent Beck:

> "Rather than asking an object for data and acting on that data, we should tell an object what to do."
>
> *Ao invés de pedir dados de um objeto e agir sobre esses dados, devemos dizer ao objeto o que fazer.*

Em vez de a AR principal "perguntar" detalhes da AR associada para validar, ela simplesmente "diz" para a AR associada se validar via `IsValid()`.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Qual a diferença entre composição e associação em DDD?"
- "Como validar relacionamentos entre Aggregate Roots?"
- "Por que Aggregate Roots têm ciclo de vida independente?"
- "Como implementar referências entre agregados em DDD?"

### Leitura Recomendada

- [Effective Aggregate Design - Vaughn Vernon](https://www.dddcommunity.org/library/vernon_2011/)
- [DDD Aggregates - Martin Fowler](https://martinfowler.com/bliki/DDD_Aggregate.html)
- [Implementing DDD - Vaughn Vernon](https://www.amazon.com/Implementing-Domain-Driven-Design-Vaughn-Vernon/dp/0321834577)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Fornece IsValid() usado para validar a AR associada |

## Referências no Código

- [PrimaryAggregateRoot.cs](../../../templates/Domain.Entities/AssociatedAggregateRoots/PrimaryAggregateRoot.cs) - LLM_RULE: Metadata de Aggregate Roots Associadas - Apenas IsRequired
- [PrimaryAggregateRoot.cs](../../../templates/Domain.Entities/AssociatedAggregateRoots/PrimaryAggregateRoot.cs) - Métodos `ValidateReferencedAggregateRootFor*Internal`
