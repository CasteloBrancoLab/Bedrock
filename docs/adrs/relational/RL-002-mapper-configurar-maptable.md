# RL-002: ConfigureInternal do Mapper Deve Chamar MapTable

## Status

Aceita

## Validacao Automatizada

Esta ADR sera validada pela rule de arquitetura
**RL002_MapperConfigureInternalCallsMapTableRule**, que verifica:

- Para cada Mapper (que herda `DataModelMapperBase<T>`) em `*.Mappers`,
  o metodo `ConfigureInternal` deve conter uma chamada a `MapTable`.

## Contexto

### O Problema (Analogia)

Imagine um formulario de registro que pede nome, email e senha, mas nao
pergunta para qual sistema o registro se destina. O formulario coleta
todas as informacoes, mas nao sabe onde salva-las. O campo "sistema" e
obrigatorio — sem ele, os dados ficam orfaos.

### O Problema Tecnico

O `DataModelMapperBase` gera automaticamente comandos SQL (SELECT,
INSERT, UPDATE, DELETE, COPY) a partir do mapeamento de colunas. Porem,
todos esses comandos precisam do nome da tabela para serem validos.

Se `ConfigureInternal` mapeia colunas via `MapColumn` mas esquece de
chamar `MapTable`, o mapper nao sabe para qual tabela gerar SQL. Isso
causa:

1. **NullReferenceException em runtime**: O `TableName` sera `null`,
   causando erro ao gerar qualquer comando SQL.
2. **Erro silencioso**: Se o erro so aparece quando a primeira query e
   executada, pode passar despercebido em code review.

## A Decisao

Todo `ConfigureInternal` de um Mapper deve chamar `MapTable` para
definir o schema e o nome da tabela:

```csharp
protected override void ConfigureInternal(
    MapperOptions<UserDataModel> mapperOptions)
{
    mapperOptions
        .MapTable(schema: "public", name: "auth_users")  // Obrigatorio
        .MapColumn(static x => x.Username)
        .MapColumn(static x => x.Email);
}
```

### Por Que Funciona

- **Fail-fast**: A regra de arquitetura detecta a ausencia de `MapTable`
  em tempo de build, antes de chegar a runtime.
- **Clareza**: O nome da tabela fica explicito no mapper, facilitando
  code review e rastreabilidade.
- **Consistencia**: Todo mapper segue o mesmo padrao — `MapTable`
  primeiro, `MapColumn` em seguida.

## Consequencias

### Beneficios

- Erros de configuracao detectados em tempo de build.
- Nomes de tabela rastreavaeis diretamente no codigo do mapper.
- Zero risco de `NullReferenceException` por tabela nao configurada.

### Trade-offs

- **Uma chamada obrigatoria**: O desenvolvedor deve lembrar de chamar
  `MapTable`. Na pratica, o fluent API incentiva naturalmente isso
  como primeira chamada da chain.

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Persistence.PostgreSql | Define `MapperOptions<T>.MapTable()` |

## Referencias no Codigo

- Mapper de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/Mappers/UserDataModelMapper.cs`
- ADR relacionada: [RL-001 — Mapper Herda DataModelMapperBase](./RL-001-mapper-herda-datamodelmapperbase.md)
