# DE-049: Métodos *Internal Protegidos em Classes Abstratas

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma franquia de restaurantes. A matriz (classe abstrata) define receitas completas: "Hambúrguer Clássico" inclui pão, carne, queijo, alface, tomate - tudo em ordem específica com tempos de preparo definidos.

A filial (classe derivada) pode usar essas receitas para compor seus próprios combos ("Combo Família = 2x Hambúrguer Clássico + Batata + Refrigerante"), mas não pode alterar os ingredientes individuais de cada receita.

Se a filial pudesse acessar ingredientes diretamente ("só o pão", "só a carne"), poderia montar um "hambúrguer" sem carne ou com ingredientes fora de ordem - quebrando o padrão de qualidade da matriz.

### O Problema Técnico

Em classes abstratas, métodos `*Internal` precisam ser acessíveis às classes filhas para que estas possam compor suas próprias operações de negócio (como `RegisterNew`).

A questão é: qual visibilidade usar?

```csharp
public abstract class Person
{
    // Opção 1: Privado (como em classes concretas)
    private bool ChangeNameInternal(...) { ... }  // ❌ Filha não consegue acessar

    // Opção 2: Protegido
    protected bool ChangeNameInternal(...) { ... }  // ✅ Filha pode usar
}
```

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos projetos não fazem distinção entre `*Internal` e `Set*`, deixando ambos como protected:

```csharp
// Abordagem comum - tudo protected
public abstract class Person
{
    protected bool SetFirstName(...) { ... }
    protected bool SetLastName(...) { ... }
    protected bool ChangeNameInternal(...) { ... }
}
```

### Por Que Não Funciona Bem

Já documentado na ADR DE-047: `Set*` protected permite que a filha quebre invariantes da classe pai.

A pergunta que esta ADR responde é: por que `*Internal` pode ser protected se `Set*` não pode?

## A Decisão

### Nossa Abordagem

Em classes abstratas, métodos `*Internal` são **PROTEGIDOS** (diferente de classes concretas onde são privados):

```csharp
public abstract class Person
{
    // *Internal PROTEGIDO - acessível às filhas
    protected bool ChangeNameInternal(ExecutionContext ctx, string firstName, string lastName)
    {
        return SetFirstName(ctx, firstName)
            & SetLastName(ctx, lastName)
            & SetFullName(ctx, $"{firstName} {lastName}");
    }

    // Set* PRIVADO - inacessível às filhas
    private bool SetFirstName(...) { ... }
    private bool SetLastName(...) { ... }
    private bool SetFullName(...) { ... }
}

public sealed class Employee : Person
{
    public static Employee? RegisterNew(ExecutionContext ctx, ...)
    {
        var instance = new Employee();

        // ✅ Pode usar *Internal protegido
        instance.ChangeNameInternal(ctx, firstName, lastName);

        // ❌ Não pode usar Set* privado
        // instance.SetFirstName(ctx, firstName);  // NÃO COMPILA

        return instance;
    }
}
```

### Diferença Entre Classes Concretas e Abstratas

| Tipo de Classe | Método `*Internal` | Razão |
|----------------|-------------------|-------|
| **Sealed (concreta)** | `private` | Ninguém herda, então não precisa ser acessível |
| **Abstract** | `protected` | Filhas precisam acessar para compor suas operações |

### Por Que *Internal Protegido Não Quebra Encapsulamento

A diferença fundamental entre `*Internal` e `Set*`:

| Aspecto | `Set*` | `*Internal` |
|---------|--------|-------------|
| **Granularidade** | Uma propriedade | Operação de negócio completa |
| **Invariantes** | Pode deixar estado parcial | Garante estado consistente |
| **Controle** | Filha decide o que chamar | Pai define a operação |

```csharp
// Set* permite escolher o que chamar (PERIGOSO)
instance.SetLastName(ctx, "Silva");  // Esqueceu SetFirstName - FullName quebrado

// *Internal é uma operação atômica (SEGURO)
instance.ChangeNameInternal(ctx, "João", "Silva");  // FirstName, LastName E FullName atualizados
```

## Consequências

### Benefícios

- **Composição Segura**: Filhas podem compor operações usando blocos completos
- **Encapsulamento Preservado**: Pai controla COMO seu estado é alterado
- **Reutilização**: Mesma lógica de negócio usada por múltiplas filhas
- **Consistência**: Impossível para a filha criar estado parcialmente atualizado

### Trade-offs

- **Mais Métodos Protegidos**: Cada operação de negócio precisa de um `*Internal`

### Relação com ADR DE-047

Esta ADR complementa DE-047 (Set* Privados):

| ADR | Regra | Razão |
|-----|-------|-------|
| DE-047 | `Set*` é PRIVADO | Granular demais - permite estado inconsistente |
| DE-049 | `*Internal` é PROTEGIDO | Operação completa - garante consistência |

Juntas, as regras formam um padrão coeso:
- Filha acessa `*Internal` (operações de negócio completas)
- `*Internal` chama `Set*` (acessível apenas dentro da classe pai)
- Invariantes são sempre mantidas

## Fundamentação Teórica

### Template Method Pattern (GoF)

O padrão Template Method define o esqueleto de um algoritmo na classe base, permitindo que subclasses redefinam certos passos.

Nossa abordagem é similar, mas invertida:
- A classe pai define operações completas (`*Internal`)
- A classe filha **usa** essas operações, não as redefine
- Passos internos (`Set*`) são privados e não podem ser alterados

### Princípio de Hollywood

"Don't call us, we'll call you" - a classe filha não chama setters individuais, ela usa operações completas que a classe pai definiu.

### Composição sobre Herança

Embora use herança, o padrão promove composição de comportamentos:
- A filha não herda implementações de `Set*`
- A filha compõe seu comportamento usando operações `*Internal` como blocos

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Qual a diferença entre Template Method e nossa abordagem de *Internal protegido?"
- "Como o princípio de Hollywood se aplica a hierarquias de entidades?"
- "Por que granularidade importa para encapsulamento?"

### Leitura Recomendada

- [Template Method Pattern - GoF](https://refactoring.guru/design-patterns/template-method)
- [Hollywood Principle](https://en.wikipedia.org/wiki/Hollywood_principle)
- ADR DE-047: Métodos Set* Privados em Classes Abstratas
- ADR DE-021: Métodos Públicos vs Métodos Internos (*Internal)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Classe base que define a infraestrutura para o padrão |

## Referências no Código

- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - comentário LLM_GUIDANCE sobre métodos *Internal em classes abstratas
- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - declaração `protected bool ChangeSamplePropertyInternal`
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - comparação com `private` em classe concreta
