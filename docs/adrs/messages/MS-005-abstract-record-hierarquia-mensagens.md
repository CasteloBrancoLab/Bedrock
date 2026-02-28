# MS-005: Abstract Record Para Hierarquia de Mensagens

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine que você tem três tipos de documentos oficiais: petições (pedidos), sentenças (decisões) e consultas (perguntas). Todos compartilham um cabeçalho padrão (número do processo, data, partes envolvidas). Cada tipo adiciona seu conteúdo específico. O cabeçalho é padronizado uma vez — não copiado em cada documento.

### O Problema Técnico

Commands, Events e Queries compartilham o mesmo envelope (`MessageMetadata`), mas precisam:
- Ter hierarquia de tipos (para pattern matching, routing, constraints genéricos)
- Ser imutáveis (mensagens são fatos, não mudam)
- Suportar herança posicional (tipos concretos adicionam campos de payload)

`record struct` não suporta herança. `class` não tem igualdade por valor nem `with` expressions nativas. Precisamos de `abstract record` (heap-allocated) para a hierarquia.

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos frameworks usam classes abstratas tradicionais:

```csharp
public abstract class MessageBase
{
    public Guid MessageId { get; set; }
    public string SchemaName { get; set; }
    // ... setters mutáveis
}

public class UserRegisteredEvent : EventBase
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
}
```

### Por Que Não Funciona Bem

- **Mutabilidade**: Setters públicos permitem alteração após construção
- **Sem igualdade por valor**: Comparar mensagens exige override manual de `Equals`/`GetHashCode`
- **Boilerplate**: `ToString()` precisa ser implementado manualmente
- **Sem `with` expressions**: Criar cópias com alterações exige construtores manuais

## A Decisão

### Nossa Abordagem

A hierarquia usa `abstract record` em cadeia:

```
IMessage (interface)
└── MessageBase (abstract record) ← auto-computa SchemaName
    ├── CommandBase (abstract record) + ICommand
    ├── EventBase (abstract record) + IEvent
    └── QueryBase (abstract record) + IQuery
```

Cada base recebe `MessageMetadata` e repassa para a base superior:

```csharp
public abstract record MessageBase : IMessage
{
    public MessageMetadata Metadata { get; }
    protected MessageBase(MessageMetadata metadata)
    {
        Metadata = metadata with { SchemaName = GetType().FullName! };
    }
}

public abstract record EventBase(MessageMetadata Metadata)
    : MessageBase(Metadata), IEvent;

// Tipo concreto — limpo, só payload
public sealed record UserRegisteredEvent(
    MessageMetadata Metadata,
    Guid UserId,
    string Email
) : EventBase(Metadata);
```

### Por Que Funciona Melhor

1. **Imutabilidade nativa**: `record` é imutável por padrão
2. **Igualdade por valor**: Dois eventos com mesmos dados são iguais sem override manual
3. **`with` expressions**: `evt with { Metadata = newMetadata }` funciona nativamente
4. **Herança posicional**: Tipos concretos adicionam campos de payload na assinatura do construtor
5. **Pattern matching**: `if (message is EventBase evt)` funciona naturalmente

## Consequências

### Benefícios

- **Type safety**: Constraints genéricos como `where T : ICommand` funcionam
- **Pattern matching**: Switch expressions por tipo de mensagem
- **Imutabilidade**: Garantida pelo compilador
- **ToString() automático**: Útil para logging e debugging

### Trade-offs (Com Perspectiva)

- **Heap allocation**: `record` (não struct) aloca no heap
  - Mensagens que cruzam fronteiras são serializadas (JSON, Protobuf). O custo de serialização e IO de rede é ordens de magnitude maior que uma alocação heap. A escolha é irrelevante para o cenário
- **`record struct` descartado**: Não suporta herança — impossível ter `EventBase : MessageBase`
  - A hierarquia é fundamental para roteamento e type safety. Sem herança, cada tipo concreto teria que reimplementar o envelope
- **`abstract` impede deserialização direta**: `Deserialize<MessageBase>` falha
  - Resolvido por `MessageEnvelope` (ver MS-004)

## Fundamentação Teórica

### Padrões de Design Relacionados

**Template Method (GoF)**: `MessageBase` define o algoritmo de construção (auto-computar SchemaName), delegando o payload para subclasses. As bases intermediárias (`CommandBase`, `EventBase`, `QueryBase`) adicionam a marker interface sem lógica adicional.

### O Que o DDD Diz

No DDD tático, Commands, Events e Queries são conceitos distintos com semânticas diferentes:
- **Commands**: Intenção (pode ser rejeitada)
- **Events**: Fato passado (imutável, não rejeitável)
- **Queries**: Consulta (sem side-effects)

A hierarquia de tipos reflete essas semânticas no sistema de tipos do C#, permitindo que o compilador enforceConstraints.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Quais são as diferenças entre record e record struct em C#?"
- "Por que abstract record é preferível a abstract class para mensagens imutáveis?"
- "Como record com herança funciona para positional parameters?"

### Leitura Recomendada

- [C# Records - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record)
- [Records with Inheritance in C# 10+](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/records)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Messages | MessageBase, CommandBase, EventBase, QueryBase implementam a hierarquia |

## Referências no Código

- [MessageBase.cs](../../../src/BuildingBlocks/Messages/MessageBase.cs) - `abstract record MessageBase : IMessage`
- [CommandBase.cs](../../../src/BuildingBlocks/Messages/Commands/CommandBase.cs) - `abstract record CommandBase : MessageBase, ICommand`
- [EventBase.cs](../../../src/BuildingBlocks/Messages/Events/EventBase.cs) - `abstract record EventBase : MessageBase, IEvent`
- [QueryBase.cs](../../../src/BuildingBlocks/Messages/Queries/QueryBase.cs) - `abstract record QueryBase : MessageBase, IQuery`
