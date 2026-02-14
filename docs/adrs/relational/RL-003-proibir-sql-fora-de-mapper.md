# RL-003: Proibir SQL Literal Fora de Mappers

## Status

Aceita

## Validacao Automatizada

Esta ADR sera validada pela rule de arquitetura
**RL003_NoSqlLiteralsOutsideMappersRule**, que verifica:

- Em projetos `Infra.Data.{Tech}`, nenhuma classe fora do namespace
  `*.Mappers` pode conter string literals com 2 ou mais keywords SQL
  (SELECT, INSERT, UPDATE, DELETE, FROM, WHERE, JOIN, etc.).

## Contexto

### O Problema (Analogia)

Imagine uma fabrica onde cada operario pode escrever suas proprias
instrucoes de montagem em qualquer papel avulso. Sem um manual
centralizado, instrucoes contraditorias proliferam, erros sao
introduzidos e ninguem sabe qual versao e a correta. Um manual unico
e oficial elimina ambiguidade.

### O Problema Tecnico

Quando SQL literal e permitido em qualquer classe do projeto (repositorios,
adapters, factories), surgem riscos:

1. **SQL Injection**: Strings concatenadas com input do usuario sao
   vulneraveis. O `DataModelMapperBase` usa expression trees e
   parametrizacao forcada — nao aceita SQL cru.
2. **SQL espalhado**: Queries duplicadas em multiplos repositorios
   divergem silenciosamente. Uma correcao em um lugar nao propaga
   para os demais.
3. **Filtros esquecidos**: Sem o mapper, cada query precisa lembrar
   de filtrar por `TenantCode` e verificar `EntityVersion`. O mapper
   injeta esses filtros automaticamente.

## A Decisao

SQL literal (strings contendo 2+ keywords SQL) e proibido fora do
namespace `*.Mappers`. Todo SQL deve ser gerado via API type-safe
do `DataModelMapperBase`:

```csharp
// ERRADO — SQL no repositorio
public class UserRepository
{
    private const string Query =
        "SELECT id FROM users WHERE email = @email"; // Violacao!
}

// CORRETO — Usar API type-safe do mapper
public class UserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        WhereClause where = _mapper.Where(
            static (UserDataModel x) => x.Email);
        string sql = _mapper.GenerateSelectCommand(where);
        // ...
    }
}
```

### Por Que Funciona

- **SQL injection impossivel**: A API do mapper nao aceita strings —
  so expression trees e valores tipados.
- **SQL centralizado**: Toda geracao de SQL passa pelo mapper, que
  injeta automaticamente filtros de tenant e version check.
- **Deteccao automatica**: A regra de arquitetura detecta SQL literal
  fora de Mappers em tempo de build.

## Consequencias

### Beneficios

- Zero risco de SQL injection por concatenacao em repositorios.
- Filtros de tenant e version check nunca esquecidos.
- SQL gerado e type-safe — erros de nome de coluna detectados em
  compile-time.

### Trade-offs

- **Curva de aprendizado**: Desenvolvedores acostumados com SQL manual
  precisam aprender a API do mapper (Where, OrderBy, GenerateSelectCommand).
- **Strings de configuracao**: Strings como nomes de connection string
  keys nao sao afetadas (menos de 2 keywords SQL).

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Persistence.PostgreSql | Define `DataModelMapperBase<T>` com API type-safe |

## Referencias no Codigo

- Mapper de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/Mappers/UserDataModelMapper.cs`
- ADR relacionada: [RL-001 — Mapper Herda DataModelMapperBase](./RL-001-mapper-herda-datamodelmapperbase.md)
- ADR relacionada: [RL-002 — ConfigureInternal Deve Chamar MapTable](./RL-002-mapper-configurar-maptable.md)
