# PG-001: MapBinaryImporter Deve Escrever Todas as Colunas

## Status

Aceita

## Validacao Automatizada

Esta ADR sera validada pela rule de arquitetura
**PG001_MapBinaryImporterWritesAllColumnsRule**, que verifica:

- Para cada Mapper (que herda `DataModelMapperBase<T>`), o numero de
  chamadas `importer.Write()` em `MapBinaryImporter` deve ser igual a
  10 (colunas base do `DataModelBase`) + N (colunas mapeadas por
  `MapColumn` em `ConfigureInternal`).

## Contexto

### O Problema (Analogia)

Imagine um formulario de remessa que exige exatamente 14 campos
preenchidos, na ordem correta. Se o remetente preencher 12 campos e
esquecer 2, o pacote e rejeitado na triagem. Se preencher na ordem
errada, o conteudo vai para o destino errado. O formulario e rigido
por design — nao por burocracia.

### O Problema Tecnico

O protocolo COPY binario do PostgreSQL (via `NpgsqlBinaryImporter`)
exige que a quantidade de `Write()` calls corresponda exatamente ao
numero de colunas definidas no comando COPY. Se houver divergencia:

1. **Menos colunas**: O PostgreSQL rejeita o bloco binario com erro
   "unexpected EOF" ou dados corrompidos.
2. **Mais colunas**: O PostgreSQL rejeita com "extra data after last
   expected column".
3. **Ordem errada**: Os dados sao escritos na coluna errada — sem
   erro, mas com dados silenciosamente corrompidos.

O `DataModelMapperBase` gera automaticamente o comando COPY com
**todas** as colunas (10 base + N especificas). O `MapBinaryImporter`
deve escrever na mesma ordem e quantidade.

### Colunas Base (DataModelBase) — 10 colunas

| # | Propriedade | Tipo |
|---|-------------|------|
| 1 | Id | Guid |
| 2 | TenantCode | Guid |
| 3 | CreatedBy | string |
| 4 | CreatedAt | DateTimeOffset |
| 5 | LastChangedBy | string? |
| 6 | LastChangedAt | DateTimeOffset? |
| 7 | LastChangedExecutionOrigin | string? |
| 8 | LastChangedCorrelationId | Guid? |
| 9 | LastChangedBusinessOperationCode | string? |
| 10 | EntityVersion | long |

## A Decisao

O `MapBinaryImporter` deve ter exatamente `10 + N` chamadas a
`importer.Write()`, onde N e o numero de `MapColumn` em
`ConfigureInternal`:

```csharp
protected override void ConfigureInternal(
    MapperOptions<UserDataModel> mapperOptions)
{
    mapperOptions
        .MapTable(schema: "public", name: "auth_users")
        .MapColumn(static x => x.Username)    // +1
        .MapColumn(static x => x.Email)       // +2
        .MapColumn(static x => x.PasswordHash) // +3
        .MapColumn(static x => x.Status);     // +4 = 4 MapColumn
}

public override void MapBinaryImporter(
    NpgsqlBinaryImporter importer, UserDataModel model)
{
    // 10 colunas base
    importer.Write(model.Id, NpgsqlDbType.Uuid);
    importer.Write(model.TenantCode, NpgsqlDbType.Uuid);
    importer.Write(model.CreatedBy, NpgsqlDbType.Varchar);
    importer.Write(model.CreatedAt, NpgsqlDbType.TimestampTz);
    importer.Write(model.LastChangedBy, NpgsqlDbType.Varchar);
    importer.Write(model.LastChangedAt, NpgsqlDbType.TimestampTz);
    importer.Write(model.LastChangedExecutionOrigin, NpgsqlDbType.Varchar);
    importer.Write(model.LastChangedCorrelationId, NpgsqlDbType.Uuid);
    importer.Write(model.LastChangedBusinessOperationCode, NpgsqlDbType.Varchar);
    importer.Write(model.EntityVersion, NpgsqlDbType.Bigint);

    // 4 colunas especificas (= MapColumn count)
    importer.Write(model.Username, NpgsqlDbType.Varchar);
    importer.Write(model.Email, NpgsqlDbType.Varchar);
    importer.Write(model.PasswordHash, NpgsqlDbType.Bytea);
    importer.Write(model.Status, NpgsqlDbType.Smallint);
    // Total: 14 Write() = 10 base + 4 MapColumn ✓
}
```

## Consequencias

### Beneficios

- Divergencia entre COPY e Write detectada em tempo de build.
- Zero risco de dados corrompidos por colunas faltantes ou extras.
- Code agents sabem exatamente quantos Write() adicionar ao gerar
  um novo mapper.

### Trade-offs

- **Manutencao manual**: Ao adicionar uma coluna, o desenvolvedor
  deve adicionar tanto o `MapColumn` quanto o `importer.Write()`.
  A regra de arquitetura garante que nao vai esquecer.
- **AutoMapColumns**: Quando `AutoMapColumns` e usado, a contagem
  estatica nao e possivel e a regra e ignorada (skip).

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Persistence.PostgreSql | Define `DataModelMapperBase<T>.MapBinaryImporter` e `MapDataModelBaseColumns` |

## Referencias no Codigo

- Mapper de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/Mappers/UserDataModelMapper.cs`
- ADR relacionada: [RL-001 — Mapper Herda DataModelMapperBase](../relational/RL-001-mapper-herda-datamodelmapperbase.md)
- ADR relacionada: [RL-002 — ConfigureInternal Deve Chamar MapTable](../relational/RL-002-mapper-configurar-maptable.md)
