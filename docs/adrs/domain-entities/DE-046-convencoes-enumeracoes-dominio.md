# DE-046: Convenções de Enumerações no Domínio

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um formulário de cadastro com campo "Estado Civil" onde as opções são: Solteiro, Casado, Divorciado. Se cada desenvolvedor criar esse campo de forma diferente - um usa números 0, 1, 2; outro usa 1, 2, 3; outro deixa o compilador decidir - em pouco tempo o banco de dados terá valores inconsistentes e ninguém saberá o que "0" significa em cada tabela.

Enumerações sem convenções claras causam:
- Valores incompatíveis entre sistemas
- Quebra de dados quando a ordem dos membros muda
- Desperdício de memória usando `int` quando `byte` bastaria
- Confusão entre "valor não definido" e "primeiro valor válido"

### O Problema Técnico

Em C#, enumerações têm comportamentos default que causam problemas:

```csharp
// Problemas com defaults do C#
public enum OrderStatus  // Usa int (4 bytes) por padrão
{
    Pending,    // = 0 (valor implícito)
    Approved,   // = 1
    Shipped     // = 2
}
```

Isso causa:
- **Desperdício de memória**: `int` usa 4 bytes quando `byte` (1 byte) bastaria
- **Valores implícitos**: Se alguém adicionar `Draft` no início, todos os valores mudam
- **Zero como primeiro valor**: `default(OrderStatus)` retorna `Pending`, impossível distinguir "não definido" de "Pending"

## Como Normalmente É Feito

### Abordagem Tradicional

A maioria dos projetos deixa o compilador decidir tudo:

```csharp
// Abordagem comum - tudo implícito
public enum CategoryTypeEnum  // Sufixo desnecessário
{
    TypeA,    // = 0 (implícito)
    TypeB,    // = 1 (implícito)
    TypeC     // = 2 (implícito)
}
```

### Por Que Não Funciona Bem

1. **Quebra de compatibilidade**: Adicionar um membro no meio reordena todos os valores:

```csharp
// Antes
public enum Status { Pending, Approved }  // Pending=0, Approved=1

// Depois de "melhoria"
public enum Status { Draft, Pending, Approved }  // Draft=0, Pending=1, Approved=2

// Dados antigos com valor 1 agora significam Pending, não Approved!
```

2. **Ambiguidade com zero**: Não há como saber se `0` é intencional ou valor default:

```csharp
var entity = new Entity();
if (entity.Status == OrderStatus.Pending)  // True mesmo sem ter definido!
{
    // Bug silencioso - nunca foi explicitamente definido como Pending
}
```

3. **Nomenclatura poluída**: Sufixo "Enum" é redundância desnecessária:

```csharp
// O tipo da variável já indica que é enum
OrderStatusEnum status = OrderStatusEnum.Pending;  // "Enum" aparece 3x!
```

## A Decisão

### Nossa Abordagem

Enumerações de domínio DEVEM seguir quatro regras:

```csharp
// ✅ Todas as convenções aplicadas
public enum CategoryType : byte  // Tipo explícito, sem sufixo
{
    TypeA = 1,  // Valor explícito, começando em 1
    TypeB = 2
}
```

### Regra 1: Nomenclatura - Sem Sufixo "Enum"

O nome da enum DEVE ser simples e direto:

```csharp
// ✅ Correto
public enum PersonType { }
public enum OrderStatus { }
public enum PaymentMethod { }

// ❌ Incorreto
public enum PersonTypeEnum { }
public enum OrderStatusEnumeration { }
```

**Razão**: O contexto de uso já indica que é uma enumeração. O sufixo é redundância.

### Regra 2: Tipo Subjacente - Usar byte Quando Possível

SEMPRE especifique o tipo subjacente explicitamente:

```csharp
// ✅ Correto - tipo explícito
public enum CategoryType : byte { }   // 1 byte, até 255 valores
public enum LargeEnum : short { }     // 2 bytes, até ±32k valores

// ❌ Incorreto - usa int por padrão
public enum CategoryType { }          // 4 bytes desnecessários
```

**Preferência de tipos**:
- `byte` (0-255): maioria dos casos, até 255 valores
- `short` (±32k): quando byte não é suficiente
- `int`: apenas quando necessário compatibilidade externa

**Razão**: Otimização de memória em coleções grandes e serialização.

### Regra 3: Valores Explícitos - Sempre Definir

SEMPRE defina valores explicitamente para cada membro:

```csharp
// ✅ Correto - valores explícitos
public enum OrderStatus : byte
{
    Pending = 1,
    Approved = 2,
    Shipped = 3,
    Cancelled = 4
}

// ❌ Incorreto - valores implícitos
public enum OrderStatus : byte
{
    Pending,    // = 0 implícito
    Approved,   // = 1 implícito
    Shipped,    // = 2 implícito
    Cancelled   // = 3 implícito
}
```

**Razão**: Evita quebra de compatibilidade se a ordem dos membros for alterada. Valores persistidos em banco permanecem consistentes.

### Regra 4: Valor Inicial - Começar em 1 (Não Zero)

SEMPRE comece valores em 1, NÃO em 0:

```csharp
// ✅ Correto - começa em 1
public enum PersonType : byte
{
    Individual = 1,
    LegalEntity = 2
}

// ❌ Incorreto - começa em 0
public enum PersonType : byte
{
    Individual = 0,  // Confunde com "não definido"
    LegalEntity = 1
}
```

**Razão**: Zero é o valor default de tipos numéricos. Começar em 1 permite distinguir entre "valor não definido" (0) e "primeiro valor válido" (1).

**Exceção**: Use 0 apenas para representar explicitamente "Unknown" ou "None":

```csharp
// ✅ Exceção válida - zero representa ausência intencional
public enum Gender : byte
{
    NotSpecified = 0,  // Explicitamente "não informado"
    Male = 1,
    Female = 2,
    Other = 3
}
```

### Por Que Funciona Melhor

1. **Estabilidade**: Valores explícitos nunca mudam acidentalmente
2. **Detectabilidade**: Zero indica "não definido", facilitando validação
3. **Eficiência**: `byte` usa 1/4 da memória de `int`
4. **Clareza**: Sem sufixos redundantes, código mais limpo

## Consequências

### Benefícios

- **Compatibilidade**: Dados persistidos permanecem válidos mesmo com adições
- **Performance**: Menor uso de memória em coleções e serialização
- **Validação**: Fácil detectar valores não inicializados (== 0)
- **Legibilidade**: Nomes limpos sem sufixos desnecessários

### Trade-offs

- **Disciplina**: Requer atenção ao adicionar novos valores (definir explicitamente)
- **Convenção**: Time precisa conhecer e seguir as regras

### Alternativas Consideradas

**Usar Smart Enums (classes)**:

```csharp
public sealed class OrderStatus
{
    public static readonly OrderStatus Pending = new(1, "Pending");
    public static readonly OrderStatus Approved = new(2, "Approved");

    public int Id { get; }
    public string Name { get; }

    private OrderStatus(int id, string name) { Id = id; Name = name; }
}
```

Smart Enums oferecem mais flexibilidade (métodos, comportamentos), mas:
- Maior complexidade para casos simples
- Serialização mais complexa
- Overhead de memória (objetos vs valores)

Para a maioria dos casos, enums nativas com convenções são suficientes.

## Fundamentação Teórica

### Convenções do .NET

O [.NET Framework Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/enum) recomenda:
- Não usar sufixo "Enum" ou "Flags" no nome
- Usar singular para enums simples (não flags)
- Considerar o tipo subjacente para otimização

### Database Persistence

Valores explícitos são críticos para persistência:
- Entity Framework mapeia o valor numérico
- Se valores mudam, registros existentes ficam inconsistentes
- Zero como "não definido" facilita validação de campos obrigatórios

### Memory Optimization

Em coleções grandes, a diferença é significativa:

| Tipo | Tamanho | 1M registros |
|------|---------|--------------|
| byte | 1 byte  | ~1 MB        |
| int  | 4 bytes | ~4 MB        |

Para enums com poucos valores (< 256), `byte` é sempre preferível.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Qual a diferença entre enum e smart enum em C#?"
- "Como o Entity Framework persiste enums no banco de dados?"
- "Quais são as boas práticas para versionamento de enums em APIs?"
- "Como usar [Flags] corretamente em enums?"

### Leitura Recomendada

- [.NET Enum Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/enum)
- [Effective C# - Item 8: Use the Null Conditional Operator](https://www.oreilly.com/library/view/effective-c-50/9780134579290/)
- [Smart Enums in C#](https://ardalis.com/enum-alternatives-in-c/)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Entidades usam enums para representar estados e tipos, seguindo estas convenções |

## Referências no Código

- [CategoryType.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Enums/CategoryType.cs) - comentários LLM_RULE sobre convenções de enums
- [PersonType.cs](../../../src/ShopDemo/Orders/Domain.Entities/Customers/Enums/PersonType.cs) - exemplo de enum seguindo as convenções
- [CustomerStatus.cs](../../../src/ShopDemo/Orders/Domain.Entities/Customers/Enums/CustomerStatus.cs) - exemplo de enum de status seguindo as convenções
