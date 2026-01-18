# DE-044: Antipadrão - Coleções Não Têm Método Set

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma escola onde os alunos são matriculados individualmente, com verificação de documentos, histórico escolar, vagas disponíveis. Agora imagine se alguém pudesse simplesmente **substituir toda a lista de alunos** de uma vez, sem passar por nenhum desses processos.

De repente, alunos sem documentos, duplicatas, excesso de vagas - todo o controle de matrícula seria bypassado. A escola precisa controlar **cada entrada e saída** de alunos, não permitir substituição em massa.

### O Problema Técnico

Métodos que substituem a coleção inteira bypassam todas as regras de negócio:

```csharp
// ❌ ANTIPADRÃO: Método que substitui a coleção
public void SetItems(List<OrderItem> items)
{
    _items = items;  // Substitui tudo!
}

// Ou via property setter:
public List<OrderItem> Items
{
    get => _items;
    set => _items = value;  // ❌ Substitui tudo!
}

// Problemas:
// 1. Nenhuma validação dos itens
// 2. Nenhuma verificação de duplicatas
// 3. Nenhum limite de quantidade
// 4. Bypass completo de regras de negócio
```

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos projetos expõem setters de coleção para facilitar ORMs ou serialização:

```csharp
public class Order
{
    public List<OrderItem> Items { get; set; } = [];
}

// Uso problemático:
order.Items = externalList;  // Substitui sem validação
order.Items = [];            // Limpa sem verificar regras
order.Items = null;          // NullReferenceException depois
```

Ou criam método `SetItems`:

```csharp
public void SetItems(IEnumerable<OrderItem> items)
{
    _items.Clear();
    _items.AddRange(items);
    // Talvez valide depois... talvez não
}
```

### Por Que Não Funciona Bem

1. **Bypass de regras**: Qualquer validação pode ser ignorada
2. **Estado inconsistente**: Pode violar invariantes do agregado
3. **Difícil de rastrear**: Não há auditoria de quem/quando/por que mudou
4. **Frágil**: Código externo pode quebrar o agregado facilmente

## A Decisão

### Nossa Abordagem

Coleções de entidades filhas **NUNCA** devem ter método `Set*`. Toda modificação deve ser através de métodos de negócio específicos:

```csharp
public sealed class CompositeAggregateRoot
    : EntityBase<CompositeAggregateRoot>,
    IAggregateRoot
{
    private readonly List<CompositeChildEntity> _compositeChildEntityCollection = [];

    // ✅ Expõe apenas leitura
    public IReadOnlyList<CompositeChildEntity> CompositeChildEntities
    {
        get { return _compositeChildEntityCollection.AsReadOnly(); }
    }

    // ❌ NUNCA faça isso:
    // public void SetCompositeChildEntities(IEnumerable<...> items) { ... }

    // ✅ Use métodos de negócio específicos:
    public CompositeAggregateRoot? RegisterNew(...) { /* adiciona na criação */ }
    public CompositeAggregateRoot? AddCompositeChildEntity(...) { /* adiciona um */ }
    public CompositeAggregateRoot? RemoveCompositeChildEntity(...) { /* remove um */ }
    public CompositeAggregateRoot? ChangeCompositeChildEntityTitle(...) { /* modifica um */ }
}
```

### Por Que Funciona Melhor

1. **Cada operação é controlada**

```csharp
// Adicionar: passa por validação
aggregateRoot.AddCompositeChildEntity(context, input);
// - Verifica se pode adicionar
// - Verifica duplicidade
// - Verifica limites
// - Registra no ExecutionContext se falhar

// Remover: passa por validação
aggregateRoot.RemoveCompositeChildEntity(context, input);
// - Verifica se existe
// - Verifica se pode remover (regras de negócio)
// - Pode impedir remoção se for o último item, etc.
```

2. **Invariantes sempre garantidas**

```csharp
// Com métodos de negócio:
// - "Não pode ter itens duplicados" - verificado em cada Add
// - "Máximo 10 itens" - verificado em cada Add
// - "Mínimo 1 item" - verificado em cada Remove
// - etc.

// Com SetItems:
// Nenhuma dessas garantias
```

3. **Auditoria e rastreabilidade**

```csharp
// Cada método público gera sua própria operação:
// - RegisterNew: cria agregado com filhos iniciais
// - AddCompositeChildEntity: adiciona um filho
// - ChangeCompositeChildEntityTitle: modifica um filho

// Fácil de logar, auditar, versionar
```

4. **Clone-modify-return preservado**

```csharp
// Cada operação retorna nova instância ou null:
var result = aggregate.AddChild(context, input);

if (result == null)
{
    // Falhou, aggregate original intacto
}
else
{
    // Sucesso, result é nova instância com o filho adicionado
}
```

## Consequências

### Benefícios

- **Controle total**: Aggregate Root decide o que é permitido
- **Consistência garantida**: Invariantes sempre respeitadas
- **API clara**: Cada operação tem nome e propósito definidos
- **Testável**: Cada operação pode ser testada isoladamente

### Trade-offs (Com Perspectiva)

- **Mais código**: Um método por tipo de operação
  - *Perspectiva*: É documentação viva das operações permitidas
  - *Benefício*: API autodescritiva, difícil de usar errado

- **ORMs podem reclamar**: EF Core espera setter para popular coleções
  - *Solução*: Use `CreateFromExistingInfo` para reconstituição (ver DE-017)
  - *Nota*: Reconstituição não valida, apenas reconstitui estado conhecido

```csharp
// Para reconstituição do banco:
public static CompositeAggregateRoot CreateFromExistingInfo(
    CreateFromExistingInfoInput input
)
{
    return new CompositeAggregateRoot(
        input.EntityInfo,
        input.Name,
        input.Code,
        input.CompositeChildEntities  // ✅ OK, dados já foram validados antes
    );
}
```

## Fundamentação Teórica

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003):

> "The root Entity [...] controls access to the objects within the Aggregate."
>
> *A Entidade raiz [...] controla o acesso aos objetos dentro do Agregado.*

`SetItems` seria dar acesso irrestrito, violando o papel da raiz.

Vaughn Vernon em "Implementing Domain-Driven Design":

> "All access to objects within an Aggregate must go through the Root."
>
> *Todo acesso a objetos dentro de um Agregado deve passar pela Raiz.*

### O Que o Clean Code Diz

**Principle of Least Privilege**:

Expor apenas o mínimo necessário. Não há necessidade legítima de substituir a coleção inteira.

**Defensive Programming**:

Proteger o estado interno contra modificações indesejadas.

### Outros Fundamentos

**Encapsulation** (OOP fundamental):

O estado interno da classe deve ser protegido. Setters de coleção quebram esse princípio.

**Information Hiding** (David Parnas):

Detalhes de implementação (como a coleção é armazenada) não devem ser expostos.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Como configurar EF Core para trabalhar com coleções encapsuladas?"
- "Qual a diferença entre expor comportamento vs expor dados?"
- "Como implementar Import/Bulk operations sem SetItems?"
- "Por que ORMs tradicionais conflitam com DDD?"

### Leitura Recomendada

- [DDD and Entity Framework - Julie Lerman](https://www.pluralsight.com/courses/domain-driven-design-fundamentals)
- [Encapsulating Collections - Vladimir Khorikov](https://enterprisecraftsmanship.com/posts/encapsulating-collections/)
- [Aggregate Design - Vaughn Vernon](https://www.dddcommunity.org/library/vernon_2011/)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Fornece padrão para métodos de negócio que modificam coleções |

## Referências no Código

- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - LLM_RULE: Coleções de Entidades Filhas NÃO Têm Método Set*
- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - Ausência proposital de método SetCompositeChildEntities
