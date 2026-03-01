# OB-008: Composicao sobre Heranca para OutboxWriter do BC

## Status

Aceita

## Contexto

### O Problema (Analogia)

Um motorista de entregas nao precisa de ser mecanico para conduzir uma
carrinha. Ele usa a carrinha (compoe), nao se torna uma carrinha
(herda). Se a carrinha for substituida por um modelo electrico, o
motorista continua a fazer entregas — so muda o veiculo que compoe.

### O Problema Tecnico

O building block `Outbox.Messages` fornece `MessageOutboxWriter` — uma
classe `sealed` que sabe enriquecer uma `MessageBase` num `OutboxEntry`
(gerar Id UUIDv7, extrair metadata, serializar payload, definir status
inicial). Esta classe e `sealed` deliberadamente:

1. **Nao ha comportamento variavel**: O algoritmo de enriquecimento e
   fixo — Id, TenantCode, CorrelationId, PayloadType, ContentType,
   Payload, CreatedAt, Status. Nao ha razao para override.
2. **Previne heranca fragil**: Se um BC herdasse e alterasse parte do
   fluxo, a consistencia do `OutboxEntry` seria comprometida.

Mas o BC precisa de uma **marker interface** propria (`IAuthOutboxWriter`)
para isolamento DI (OB-011). Como `MessageOutboxWriter` e `sealed`, nao
pode herdar dele. E mesmo que pudesse, herdar para apenas mudar o tipo
DI seria abuso de heranca.

## A Decisao

O writer do BC usa **composicao**: cria internamente uma instancia de
`MessageOutboxWriter` e delega a chamada:

```csharp
public sealed class AuthOutboxWriter : IAuthOutboxWriter
{
    private readonly MessageOutboxWriter _writer;

    public AuthOutboxWriter(
        IAuthOutboxRepository repository,        // marker do BC
        IOutboxSerializer<MessageBase> serializer,
        TimeProvider timeProvider)
    {
        // Composicao: cria o writer interno com as dependencias
        _writer = new MessageOutboxWriter(repository, serializer, timeProvider);
    }

    public Task EnqueueAsync(MessageBase payload, CancellationToken cancellationToken)
        => _writer.EnqueueAsync(payload, cancellationToken);
}
```

**Regras fundamentais:**

1. **Composicao, nao heranca**: `AuthOutboxWriter` contem um
   `MessageOutboxWriter`, nao estende.
2. **Marker interface do BC**: Implementa `IAuthOutboxWriter` (que
   herda `IOutboxWriter<MessageBase>`) para type safety DI.
3. **Dependencias do BC**: Recebe `IAuthOutboxRepository` (marker) —
   garante que o writer do Auth usa o repositorio do Auth.
4. **Delegacao pura**: `EnqueueAsync` delega sem logica adicional.
   O writer do BC nao adiciona comportamento.
5. **Sealed**: O writer do BC tambem e `sealed` — nao ha necessidade
   de mais extensao.

### Fluxo de composicao

```
Use Case → IAuthOutboxWriter
               │
               ▼
         AuthOutboxWriter (composicao)
               │
               ▼
         MessageOutboxWriter (logica de enriquecimento)
               │
               ├── IOutboxSerializer<MessageBase>.Serialize()
               └── IOutboxRepository.AddAsync(OutboxEntry)
                        │
                        ▼
                  AuthOutboxRepository (via IAuthOutboxRepository marker)
```

## Consequencias

### Beneficios

- Respeita a intencao de `sealed` no `MessageOutboxWriter`.
- Cada BC tem seu proprio tipo DI sem duplicar logica.
- Troca do writer interno (ex: versao com batching) requer mudanca
  num unico ponto — a composicao.
- Testavel: pode-se mockar `IAuthOutboxWriter` em unit tests sem
  depender de `MessageOutboxWriter`.

### Trade-offs (Com Perspectiva)

- **Classe "wrapper" por BC**: Parece boilerplate — ~15 linhas por BC.
  Na pratica, e uma classe minima e mecanica que qualquer code agent
  gera sem erro.
- **`new` no construtor**: `MessageOutboxWriter` e criado com `new`,
  nao injectado pelo DI. Aceite deliberadamente: o DI do BC resolve
  as dependencias (repository, serializer, timeProvider) e o wrapper
  compoe — o `MessageOutboxWriter` e um detalhe de implementacao.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Composition over Inheritance** (GoF, Design Patterns): Favorecer
  composicao de objectos sobre heranca de classes para flexibilidade.
- **Adapter** (GoF): `AuthOutboxWriter` adapta `MessageOutboxWriter`
  para a interface do BC (`IAuthOutboxWriter`).
- **Delegation** (Lieberman, 1986): Delegacao explicita de
  responsabilidade a um objecto interno.

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Outbox | Define `IOutboxWriter<TPayload>` |
| Bedrock.BuildingBlocks.Outbox.Messages | Implementa `MessageOutboxWriter` (sealed) |

## Referencias no Codigo

- Writer do BC: `src/ShopDemo/Auth/Infra.Data.PostgreSql/Outbox/AuthOutboxWriter.cs`
- Writer do BB: `src/BuildingBlocks/Outbox.Messages/MessageOutboxWriter.cs`
- Marker: `src/ShopDemo/Auth/Infra.CrossCutting.Messages/Outbox/Interfaces/IAuthOutboxWriter.cs`
- ADR relacionada: [OB-011 — Marker Interfaces por BC](./OB-011-marker-interfaces-outbox-por-bc.md)
- ADR relacionada: [IN-006 — Marker Interface para Conexao](../infrastructure/IN-006-conexao-marker-interface-herda-iconnection.md)
