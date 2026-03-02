# OB-007: Desserializacao em Duas Fases

## Status

Aceita

## Contexto

### O Problema (Analogia)

Um tradutor bilingue recebe um documento selado. Primeiro, le o rotulo
para saber em que lingua esta escrito (ingles? frances? mandarim?).
Depois, abre e traduz o conteudo usando o dicionario correcto. Se
tentasse traduzir sem saber a lingua, produziria lixo.

### O Problema Tecnico

O outbox armazena payloads como `byte[]` (OB-002) com dois
discriminadores:

- **`PayloadType`**: Identifica o tipo de mensagem (ex: nome qualificado
  do tipo .NET, SchemaName).
- **`ContentType`**: Identifica o formato de serializacao (ex:
  `application/json`).

Para desserializar, o processador precisa:

1. Resolver o `Type` .NET a partir do `PayloadType` string.
2. Desserializar os bytes para esse `Type` usando o formato indicado
   por `ContentType`.

Se o desserializador tentar desserializar para `MessageBase` (tipo
base), perde os campos especificos da mensagem concreta (ex:
`UserRegisteredEvent.Input`). Se tentar para `object`, obtem um
`JsonElement` generico sem tipagem forte.

## A Decisao

A desserializacao segue duas fases explicitas:

```csharp
public sealed class MessageOutboxDeserializer : IOutboxDeserializer
{
    private readonly IStringSerializer _serializer;

    public object? Deserialize(byte[] data, string payloadType, string contentType)
    {
        // Fase 1: Resolver Type a partir do PayloadType (SchemaName)
        var type = Type.GetType(payloadType)
                   ?? throw new InvalidOperationException(
                       $"Tipo '{payloadType}' nao encontrado.");

        // Fase 2: Desserializar bytes para o Type concreto
        return _serializer.DeserializeFromUtf8Bytes<object>(data, type);
    }
}
```

**Fase 1 — Resolucao de tipo:**

- `PayloadType` contem o nome qualificado do tipo .NET (ex:
  `ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events.UserRegisteredEvent, ShopDemo.Auth.Infra.CrossCutting.Messages`).
- Este valor vem do `SchemaName` da mensagem, computado automaticamente
  pela `MessageBase` a partir do tipo concreto.
- `Type.GetType()` resolve o tipo em runtime.

**Fase 2 — Desserializacao:**

- Com o `Type` concreto resolvido, `DeserializeFromUtf8Bytes<object>(data, type)`
  desserializa os bytes usando System.Text.Json com o tipo correcto.
- O resultado e um objecto tipado (ex: `UserRegisteredEvent`), nao um
  `JsonElement` generico.

**Regras fundamentais:**

1. **PayloadType e o discriminador primario**: Sem ele, nao ha como
   saber que tipo desserializar.
2. **Tipo concreto, nao base**: A desserializacao usa o tipo concreto
   da mensagem, preservando todos os campos.
3. **ContentType para validacao futura**: Permite suportar multiplos
   formatos (Protobuf, Avro) com desserializadores especificos.
4. **Falha rapida**: Se o tipo nao e encontrado, lanca excecao
   imediatamente — nao retorna null silenciosamente.

## Consequencias

### Beneficios

- Payload desserializado para o tipo concreto — sem perda de campos.
- Suporte a polimorfismo: diferentes mensagens na mesma tabela, cada
  uma desserializada para o seu tipo.
- Extensivel: novos formatos (Protobuf) requerem apenas novo
  desserializador, nao mudanca no fluxo.

### Trade-offs (Com Perspectiva)

- **`Type.GetType()` depende de assemblies carregados**: Se o assembly
  da mensagem nao estiver no AppDomain, a resolucao falha. Na pratica,
  o processador referencia os mesmos assemblies que o produtor.
- **Risco de rename de tipos**: Se o tipo for renomeado, entries antigas
  falham na resolucao. Versionamento de mensagens (namespace `V1`, `V2`)
  mitiga este risco — tipos antigos nao sao removidos.

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Outbox | Define `IOutboxDeserializer` com `Deserialize(byte[], string, string)` |
| Bedrock.BuildingBlocks.Outbox.Messages | Implementa `MessageOutboxDeserializer` com duas fases |
| Bedrock.BuildingBlocks.Messages | `MessageBase.SchemaName` fornece o `PayloadType` |

## Referencias no Codigo

- Interface: `src/BuildingBlocks/Outbox/Interfaces/IOutboxDeserializer.cs`
- Implementacao: `src/BuildingBlocks/Outbox.Messages/MessageOutboxDeserializer.cs`
- SchemaName: `src/BuildingBlocks/Messages/MessageBase.cs`
- ADR relacionada: [OB-002 — Payload Format-Agnostico](./OB-002-payload-bytes-agnostico-formato.md)
