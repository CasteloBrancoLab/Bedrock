# DE-048: Métodos Validate* Públicos em Classes Abstratas

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um laboratório de análises clínicas com uma matriz (classe abstrata) e filiais (classes derivadas). A matriz define os protocolos de validação de amostras - verificar temperatura, volume mínimo, prazo de coleta.

Se esses protocolos fossem "internos" (protected), apenas funcionários do laboratório poderiam verificar se uma amostra está válida. O paciente chegaria, entregaria a amostra, esperaria o processamento... e só então descobriria que estava fora do prazo.

Com protocolos públicos, a recepção (camada externa) pode verificar ANTES de aceitar a amostra: "Senhor, esta amostra foi coletada há mais de 24 horas, não podemos processar."

### O Problema Técnico

Em classes abstratas, pode parecer natural tornar métodos `Validate*` protegidos, já que a classe não será instanciada diretamente:

```csharp
// ❌ Abordagem que parece fazer sentido
public abstract class Person
{
    // "Só as classes filhas precisam validar, certo?"
    protected static bool ValidateFirstName(ExecutionContext ctx, string? firstName) { ... }
    protected static bool ValidateLastName(ExecutionContext ctx, string? lastName) { ... }
}
```

Mas isso ignora que camadas externas (controllers, serviços de aplicação) precisam validar inputs ANTES de tentar criar ou modificar entidades.

## Como Normalmente É Feito

### Abordagem Tradicional

Alguns projetos tratam validação como "assunto interno" da entidade:

```csharp
// Abordagem comum - validação protegida
public abstract class Person
{
    protected static bool ValidateFirstName(ExecutionContext ctx, string? firstName) { ... }
}

public sealed class Employee : Person
{
    public static Employee? RegisterNew(ExecutionContext ctx, string firstName)
    {
        // Validação acontece aqui dentro
        if (!ValidateFirstName(ctx, firstName))
            return null;
        // ...
    }
}
```

O controller fica assim:

```csharp
public IActionResult Create(CreateEmployeeRequest request)
{
    var employee = Employee.RegisterNew(ctx, request.FirstName);

    if (employee == null)
        return BadRequest(ctx.Messages);  // Validação falhou

    // ...
}
```

### Por Que Não Funciona Bem

**1. Validação Tardia**

O controller precisa tentar criar a entidade para descobrir se os dados são válidos. Isso pode envolver:
- Alocação de objetos desnecessária
- Chamadas a métodos que poderiam ser evitadas
- Lógica de negócio parcialmente executada antes da falha

**2. Impossível Validar Antes de Criar**

```csharp
// Controller quer validar ANTES de chamar RegisterNew
public IActionResult Create(CreateEmployeeRequest request)
{
    // ❌ NÃO COMPILA - ValidateFirstName é protected
    if (!Person.ValidateFirstName(ctx, request.FirstName))
        return BadRequest(ctx.Messages);

    var employee = Employee.RegisterNew(ctx, request.FirstName);
    // ...
}
```

**3. Duplicação de Validação**

Para contornar, desenvolvedores duplicam validação no controller:

```csharp
public IActionResult Create(CreateEmployeeRequest request)
{
    // Validação duplicada no controller
    if (string.IsNullOrEmpty(request.FirstName) || request.FirstName.Length > 255)
        return BadRequest("FirstName inválido");

    var employee = Employee.RegisterNew(ctx, request.FirstName);
    // ...
}
```

Isso viola o princípio Single Source of Truth (ADR DE-016).

## A Decisão

### Nossa Abordagem

Métodos `Validate*` são **PÚBLICOS e ESTÁTICOS** em classes abstratas, exatamente como em classes concretas:

```csharp
public abstract class Person
{
    // ✅ Públicos e estáticos - acessíveis de qualquer lugar
    public static bool ValidateFirstName(ExecutionContext ctx, string? firstName)
    {
        return ValidationUtils.ValidateIsRequired(ctx, ..., firstName)
            && ValidationUtils.ValidateMaxLength(ctx, ..., firstName?.Length ?? 0);
    }

    public static bool ValidateLastName(ExecutionContext ctx, string? lastName)
    {
        // ...
    }
}
```

Agora o controller pode validar antecipadamente:

```csharp
public IActionResult Create(CreateEmployeeRequest request)
{
    // ✅ Validação ANTES de tentar criar
    bool isValid = Person.ValidateFirstName(ctx, request.FirstName)
        & Person.ValidateLastName(ctx, request.LastName);

    if (!isValid)
        return BadRequest(ctx.Messages);

    var employee = Employee.RegisterNew(ctx, request.FirstName, request.LastName);
    // ...
}
```

### Por Que Funciona Melhor

1. **Fail-Fast**: Erros detectados no ponto de entrada, não no domínio
2. **Single Source of Truth**: Mesma lógica usada internamente e externamente
3. **Performance**: Evita alocações e processamento desnecessário
4. **Reutilização**: Qualquer camada pode validar usando os mesmos métodos

## Consequências

### Benefícios

- **Consistência**: Validação idêntica em todas as camadas
- **Validação Antecipada**: Erros detectados o mais cedo possível
- **Sem Duplicação**: Uma única implementação de cada validação
- **Testabilidade**: Métodos públicos são fáceis de testar isoladamente

### Trade-offs

- **Exposição**: Métodos de validação são visíveis para todo o sistema

### Trade-offs Frequentemente Superestimados

**"Expor validação quebra encapsulamento"**

Métodos `Validate*` são **puros** (sem side-effects) e **estáticos**. Eles não expõem estado interno da entidade - apenas aplicam regras aos parâmetros recebidos.

Expor validação não é diferente de expor metadados (que já são públicos via `Metadata`). É informação sobre as regras, não sobre o estado.

## Fundamentação Teórica

### Relação com ADRs Existentes

Esta decisão é uma extensão natural das ADRs para classes concretas:

| ADR | Regra | Aplica-se a Abstratas? |
|-----|-------|------------------------|
| DE-009 | Métodos Validate* Públicos e Estáticos | ✅ Sim |
| DE-010 | ValidationUtils para Validações Padrão | ✅ Sim |
| DE-011 | Parâmetros Validate* Nullable por Design | ✅ Sim |
| DE-016 | Single Source of Truth | ✅ Sim |

### Princípio da Menor Surpresa

Desenvolvedores esperam consistência. Se `SimpleAggregateRoot.ValidateFirstName()` é público, `AbstractPerson.ValidateFirstName()` também deve ser. Comportamento diferente baseado em "é abstrata ou concreta" viola expectativas.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Por que validação antecipada melhora a experiência do usuário?"
- "Como o princípio Single Source of Truth se aplica a validação?"
- "Qual a diferença entre encapsulamento de estado e encapsulamento de regras?"

### Leitura Recomendada

- [Fail-Fast Principle](https://en.wikipedia.org/wiki/Fail-fast)
- [Single Source of Truth](https://en.wikipedia.org/wiki/Single_source_of_truth)
- ADR DE-009: Métodos Validate* Públicos e Estáticos
- ADR DE-016: Single Source of Truth para Regras de Validação

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [ValidationUtils](../../building-blocks/core/validation-utils.md) | Utilitário usado pelos métodos Validate* para validações padrão |

## Referências no Código

- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - comentário LLM_GUIDANCE sobre métodos Validate* em classes abstratas
- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - declaração `public static bool ValidateSampleProperty`
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - padrão equivalente em classe concreta
