# OB-004: Inicializacao Lazy de SQL no Repositorio

## Status

Aceita

## Contexto

### O Problema (Analogia)

Um restaurante que imprime o menu completo no momento em que abre as
portas, mesmo antes de qualquer cliente chegar, desperica tinta e papel
se o menu mudar antes do almoco. Melhor: preparar o template do menu na
abertura e so imprimir quando o primeiro cliente pedir — uma vez impresso,
reutilizar para todos os clientes seguintes.

### O Problema Tecnico

O `OutboxPostgreSqlRepositoryBase` precisa construir statements SQL
parametrizados com o nome do schema e tabela configurados pelo BC
(via `ConfigureInternal`). Ha duas abordagens:

1. **Construir no construtor**: SQL e montado imediatamente. Se o
   repositorio for resolvido pelo DI mas nunca utilizado naquele request
   (ex: request de leitura que nao gera eventos), o trabalho de
   construcao e desperdicado.
2. **Construir no primeiro uso (lazy)**: SQL e montado quando o primeiro
   metodo e chamado. Apos a primeira execucao, os statements sao
   reutilizados (cached em campos `string`).

O custo de `string.Format` ou interpolacao para 4-5 statements SQL e
pequeno em absoluto, mas multiplicado por N requests/segundo em que o
outbox nao e usado, acumula. Mais importante: a inicializacao lazy
garante que `ConfigureInternal` ja foi chamado — o construtor da classe
derivada pode nao ter terminado quando o construtor base executa.

## A Decisao

O repositorio base usa o padrao **lazy initialization** com um metodo
`EnsureConfigured()` chamado no inicio de cada operacao:

```csharp
public abstract class OutboxPostgreSqlRepositoryBase : IOutboxRepository
{
    private string? _insertSql;
    private string? _claimSql;
    private string? _markSentSql;
    private string? _markFailedSql;

    // Template method — chamado uma unica vez
    protected abstract void ConfigureInternal(OutboxPostgreSqlOptions options);

    private void EnsureConfigured()
    {
        if (_insertSql is not null) return; // ja inicializado

        var options = new OutboxPostgreSqlOptions();
        ConfigureInternal(options);

        _insertSql = BuildInsertSql(options);
        _claimSql = BuildClaimSql(options);
        _markSentSql = BuildMarkSentSql(options);
        _markFailedSql = BuildMarkFailedSql(options);
    }

    public Task AddAsync(OutboxEntry entry, CancellationToken ct)
    {
        EnsureConfigured();
        // usa _insertSql (ja cached)
        ...
    }
}
```

**Regras fundamentais:**

1. **SQL construido uma vez**: Apos `EnsureConfigured()`, os campos
   `string` sao reutilizados em todas as chamadas subsequentes.
2. **Zero alocacao apos inicializacao**: Nenhum `string.Format` ou
   interpolacao nas chamadas seguintes.
3. **Thread-safe para scoped lifetime**: O repositorio e scoped (um
   por request), sem concorrencia intra-request.
4. **Template method**: `ConfigureInternal` e o ponto de extensao —
   o BC define schema e tabela; a base constroi o SQL.

## Consequencias

### Beneficios

- Zero overhead em requests que nao utilizam o outbox.
- SQL construido exactamente uma vez por instancia (scoped = por request).
- Padrão consistente com outros building blocks do Bedrock (ex:
  `PostgreSqlConnectionBase` usa a mesma abordagem).

### Trade-offs (Com Perspectiva)

- **Null check em cada chamada**: `_insertSql is not null` e uma
  comparacao trivial (branch prediction favorece o caminho "ja
  inicializado" apos a primeira chamada).
- **Nao e thread-safe para singleton**: Se o repositorio fosse
  singleton, dois threads poderiam inicializar simultaneamente.
  Como e scoped, este cenario nao ocorre.

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Outbox.PostgreSql | Implementa `OutboxPostgreSqlRepositoryBase` com lazy init |
| Bedrock.BuildingBlocks.Persistence.PostgreSql | Padrao analogo em `PostgreSqlConnectionBase` |

## Referencias no Codigo

- Repositorio base: `src/BuildingBlocks/Outbox.PostgreSql/OutboxPostgreSqlRepositoryBase.cs`
- Exemplo de BC: `src/ShopDemo/Auth/Infra.Data.PostgreSql/Outbox/AuthOutboxRepository.cs`
