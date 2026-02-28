# MS-002: SchemaName Auto-Computado via GetType().FullName

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine um sistema de arquivos onde cada arquivo precisa de um caminho único. Se o usuário tivesse que digitar manualmente o caminho completo a cada vez que salva um arquivo, erros de digitação seriam inevitáveis. O sistema operacional resolve isso automaticamente — ele sabe onde o arquivo está e gera o caminho.

### O Problema Técnico

Mensagens serializam e trafegam entre processos. O consumer precisa saber o tipo concreto para deserializar o payload. Esse identificador (SchemaName) precisa ser:
- Único entre versões (V1 vs V2) e Bounded Contexts
- Correto sempre — um typo impede o roteamento
- Presente no envelope para deserialização em dois estágios

Se o produtor precisa informar manualmente `SchemaName = "MyNamespace.V1.Events.UserRegisteredEvent"`, erros são inevitáveis: typos, nomes desatualizados após rename, duplicatas acidentais.

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos frameworks pedem que o desenvolvedor passe o SchemaName como string literal ou constante:

```csharp
// SchemaName manual — error-prone
public sealed record UserRegisteredEvent(
    string SchemaName,  // "MyNamespace.V1.Events.UserRegisteredEvent"
    Guid UserId,
    string Email
);

// Ou via atributo
[MessageSchema("MyNamespace.V1.Events.UserRegisteredEvent")]
public sealed record UserRegisteredEvent(Guid UserId, string Email);
```

### Por Que Não Funciona Bem

- **Typos**: `"MyNamespace.V1.Events.UserRegsteredEvent"` — falta um 'i' e o consumer não encontra o handler
- **Rename sem atualizar**: Classe renomeada mas string continua com o nome antigo
- **Duplicatas acidentais**: Dois tipos com o mesmo SchemaName manual — roteamento ambíguo
- **Boilerplate**: Cada tipo concreto precisa repetir a string

## A Decisão

### Nossa Abordagem

SchemaName é computado automaticamente por `MessageBase` usando `GetType().FullName` e injetado no `MessageMetadata` via `with` expression:

```csharp
public abstract record MessageBase : IMessage
{
    public MessageMetadata Metadata { get; }

    protected MessageBase(MessageMetadata metadata)
    {
        // Auto-computa SchemaName — produtor nunca preenche manualmente
        Metadata = metadata with { SchemaName = GetType().FullName! };
    }
}
```

O produtor cria o `MessageMetadata` com `SchemaName` vazio ou qualquer valor — `MessageBase` sobrescreve com o tipo concreto real:

```csharp
// Produtor não se preocupa com SchemaName
var evt = new UserRegisteredEvent(
    new MessageMetadata(Guid.NewGuid(), DateTimeOffset.UtcNow, "", correlationId, ...),
    userId, email
);

// evt.Metadata.SchemaName == "MyNamespace.V1.Events.UserRegisteredEvent"
```

### Por Que Funciona Melhor

1. **Zero erros de digitação**: O runtime resolve o nome real do tipo
2. **Rename-safe**: Se a classe é renomeada, o SchemaName muda automaticamente
3. **Único por design**: `FullName` inclui namespace completo — sem ambiguidade entre V1/V2 ou BCs diferentes
4. **Zero boilerplate**: Tipos concretos não precisam declarar SchemaName

## Consequências

### Benefícios

- **Confiabilidade**: Impossível ter SchemaName incorreto
- **Manutenção zero**: Nenhum desenvolvedor precisa lembrar de atualizar strings
- **Versionamento natural**: `V1.Events.UserRegisteredEvent` vs `V2.Events.UserRegisteredEvent` — namespaces já diferenciam

### Trade-offs (Com Perspectiva)

- **SchemaName muda com rename/move**: Se o tipo mudar de namespace, consumidores com mensagens antigas (no broker/event store) terão SchemaName diferente
  - Mitigação: Versionamento via namespaces (`V1/`, `V2/`) torna renames raros. Migrações de SchemaName são tratadas como breaking changes
- **Custo de `GetType().FullName`**: Uma chamada de reflection por construção de mensagem
  - Na prática, negligenciável comparado ao custo de serialização e IO de rede

## Fundamentação Teórica

### Padrões de Design Relacionados

**Convention Over Configuration**: Em vez de exigir configuração explícita (SchemaName manual), o framework deriva o valor por convenção (FullName do tipo). Reduz erros e boilerplate.

### O Que o DDD Diz

Mensagens cruzam Bounded Contexts. O SchemaName deve ser globalmente único. Usar o namespace completo (`BoundedContext.Infra.CrossCutting.Messages.V1.Events.UserRegisteredEvent`) garante unicidade entre BCs e versões — alinhado com a ênfase do DDD em linguagem ubíqua e fronteiras explícitas.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Quais são os trade-offs de usar GetType().FullName como discriminator de tipo?"
- "Como frameworks de Event Sourcing resolvem versionamento de SchemaName?"
- "Quando Convention Over Configuration pode causar problemas?"

### Leitura Recomendada

- [Event Versioning in Event-Driven Systems](https://www.eventstore.com/blog/event-versioning)
- [Schema Evolution in Apache Kafka](https://docs.confluent.io/platform/current/schema-registry/fundamentals/schema-evolution.html)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Messages | MessageBase implementa a computação automática de SchemaName |

## Referências no Código

- [MessageBase.cs](../../../src/BuildingBlocks/Messages/MessageBase.cs) - `Metadata = metadata with { SchemaName = GetType().FullName! }`
- [MessageMetadata.cs](../../../src/BuildingBlocks/Messages/MessageMetadata.cs) - campo `SchemaName` no envelope
