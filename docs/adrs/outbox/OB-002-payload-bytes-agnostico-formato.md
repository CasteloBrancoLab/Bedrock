# OB-002: Payload Format-Agnostico com byte[] e ContentType

## Status

Aceita

## Contexto

### O Problema (Analogia)

Um armazem de encomendas recebe pacotes de todos os tamanhos e formatos.
O armazem nao precisa saber se o conteudo e roupa, electronica ou comida
— apenas precisa de um codigo de barras (tipo) e um rotulo com instrucoes
de manuseio (content type). O armazem armazena o pacote como uma caixa
opaca e quem abre e o destinatario, com as instrucoes corretas.

### O Problema Tecnico

O outbox precisa persistir payloads de mensagens de integracao. As
opcoes comuns sao:

1. **`string` (JSON)**: Simples, mas acopla o outbox a JSON. Se um BC
   precisar de Protobuf, Avro ou MessagePack, o outbox nao suporta.
2. **`JsonDocument`**: Acopla ainda mais fortemente a JSON e ao
   System.Text.Json especificamente.
3. **`byte[]` + discriminador**: Agnistico — o outbox armazena bytes e o
   formato e indicado por um campo separado (`ContentType`).

Se o outbox armazenar `string`, a migacao para formatos binarios
(Protobuf, Avro) requer alterar a tabela, o repositorio e todos os
serializadores. Com `byte[]`, basta trocar o serializador — o outbox
e o repositorio nao mudam.

## A Decisao

O payload e armazenado como `byte[]` (PostgreSQL `bytea`) e o formato
e identificado pelo campo `ContentType`:

```csharp
// OutboxEntry — campos relevantes
public required string ContentType { get; init; }  // ex: "application/json"
public required byte[] Payload { get; init; }       // bytes serializados
```

O serializador declara o seu `ContentType`:

```csharp
public interface IOutboxSerializer<in TPayload>
{
    string ContentType { get; }         // declaracao explicita do formato
    byte[] Serialize(TPayload payload); // retorna bytes, nao string
}

// Implementacao JSON
public sealed class MessageOutboxSerializer : IOutboxSerializer<MessageBase>
{
    public string ContentType => "application/json";

    public byte[] Serialize(MessageBase payload)
    {
        var concreteType = payload.GetType();
        return _serializer.SerializeToUtf8Bytes(payload, concreteType)
               ?? throw new InvalidOperationException(...);
    }
}
```

**Regras fundamentais:**

1. **Payload e sempre `byte[]`**: O outbox nao interpreta o conteudo.
2. **ContentType e obrigatorio**: Sem ele, o desserializador nao sabe
   como interpretar os bytes.
3. **Serializador declara ContentType**: O `IOutboxSerializer<T>` expoe
   a propriedade para que o writer grave no entry.
4. **Desserializador usa ContentType**: Pode validar ou selecionar
   estrategia de desserializacao com base no formato.

## Consequencias

### Beneficios

- Suporte futuro a Protobuf, Avro, MessagePack sem alterar tabela ou
  repositorio.
- Compatibilidade natural com PostgreSQL `bytea` (eficiente para
  dados binarios).
- Serializadores sao plugaveis — cada BC pode usar o formato que
  quiser.

### Trade-offs (Com Perspectiva)

- **Payload nao e legivel na tabela**: `bytea` nao e inspecionavel via
  `SELECT` como JSON seria. Na pratica, debugging e feito via logs
  estruturados e telemetria (OB-012), nao por queries manuais.
- **Overhead de encode/decode UTF-8**: Para JSON, ha conversao
  `string → byte[]`. O custo e negligivel comparado com I/O de rede
  e disco.

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Outbox | Define `OutboxEntry` com `byte[] Payload` e `string ContentType` |
| Bedrock.BuildingBlocks.Outbox | Define `IOutboxSerializer<T>` com `ContentType` e `byte[] Serialize()` |
| Bedrock.BuildingBlocks.Outbox.Messages | Implementa `MessageOutboxSerializer` com ContentType `application/json` |

## Referencias no Codigo

- Interface: `src/BuildingBlocks/Outbox/Interfaces/IOutboxSerializer.cs`
- Implementacao JSON: `src/BuildingBlocks/Outbox.Messages/MessageOutboxSerializer.cs`
- Entry: `src/BuildingBlocks/Outbox/Models/OutboxEntry.cs`
- ADR relacionada: [OB-007 — Desserializacao em Duas Fases](./OB-007-desserializacao-duas-fases.md)
