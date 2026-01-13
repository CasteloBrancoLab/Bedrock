# Id - Gerador de IDs Monotonicos UUIDv7

A struct `Id` fornece geracao de identificadores unicos baseados em UUIDv7, com ordenacao temporal e garantia de monotonicidade por thread.

## Por Que Usar

| Caracteristica | `Guid.CreateVersion7()` | `Id.GenerateNewId()` |
|----------------|-------------------------|----------------------|
| Monotonico dentro do milissegundo | Nao | Sim |
| Event Sourcing/CQRS | Problematico | Ideal |
| Replay de Eventos | Ordem pode mudar | Ordem preservada |
| Protecao contra clock drift | Nao | Sim |
| Performance | ~68 ns | ~73 ns |

## Estrutura do UUIDv7

```
┌─────────────────┬──────┬─────────┬────────┬──────────────────┐
│  Timestamp (48) │ Ver  │ Counter │ Variant│   Random (46)    │
│                 │ (4)  │  (26)   │  (2)   │                  │
└─────────────────┴──────┴─────────┴────────┴──────────────────┘
```

- **Timestamp (48 bits)**: Milissegundos desde Unix epoch - garante ordenacao temporal
- **Version (4 bits)**: Sempre 7 (UUIDv7)
- **Counter (26 bits)**: Contador monotonico por thread - ate ~67 milhoes de IDs por ms
- **Variant (2 bits)**: Sempre 10 (RFC 4122)
- **Random (46 bits)**: Bytes aleatorios criptograficos - unicidade entre threads/servidores

## Uso

```csharp
// Gerar novo ID
var id = Id.GenerateNewId();

// Gerar com TimeProvider customizado (para testes)
var id = Id.GenerateNewId(timeProvider);

// Criar a partir de Guid existente
var id = Id.CreateFromExistingInfo(existingGuid);

// Conversao implicita para Guid
Guid guid = id;

// Conversao implicita de Guid
Id id = someGuid;
```

## Cenarios de Geracao

### Cenario 1: Novo milissegundo (comum)
Contador reinicia para 0, timestamp atualizado.

### Cenario 2: Clock drift (relogio retrocedeu)
Mantem ultimo timestamp valido e incrementa contador, garantindo monotonicidade.

### Cenario 3: Mesmo milissegundo (alta frequencia)
Incrementa contador. Se exceder ~67 milhoes, faz spin-wait ate proximo ms.

## Beneficios

- **Performance**: ~70-75 ns por ID
- **Ordenacao**: IDs ordenáveis por timestamp
- **Unicidade**: Garantida em ambientes distribuidos
- **Thread-safe**: Sem locks, zero contencao
- **Monotonicidade**: IDs de uma thread sempre crescentes
- **Compatibilidade**: Funciona como Guid (conversao implicita)
- **Indices eficientes**: Sem fragmentacao no banco de dados

## Limitacao Critica

**Clock Skew Futuro**: Se o relogio do sistema avançar para o futuro e depois corrigir, IDs gerados durante o periodo "futuro" terao timestamps maiores que IDs gerados depois da correcao. A monotonicidade e garantida apenas por thread, nao globalmente.
