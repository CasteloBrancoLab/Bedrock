# RL-001: Mapper Deve Herdar DataModelMapperBase

## Status

Aceita

## Validacao Automatizada

Esta ADR sera validada pela rule de arquitetura
**RL001_MapperInheritsDataModelMapperBaseRule**, que verifica:

- Para cada DataModel em `*.DataModels`, deve existir uma classe `sealed`
  no namespace `*.Mappers` que herda de
  `DataModelMapperBase<TDataModel>`.
- A classe deve sobrescrever `ConfigureInternal` e `MapBinaryImporter`.

## Contexto

### O Problema (Analogia)

Imagine um tradutor juridico certificado. Cada tipo de documento (contrato,
procuracao, certidao) tem um dicionario especifico de termos legais. O
tradutor nao inventa traducoes — consulta o dicionario oficial. Se cada
tradutor usasse termos diferentes, os documentos seriam inconsistentes e
potencialmente invalidos. O dicionario centralizado garante que "mortgage"
e sempre traduzido como "hipoteca", em qualquer documento.

### O Problema Tecnico

O acesso a dados tem dois riscos criticos que nao podem depender de
disciplina individual do desenvolvedor:

1. **SQL Injection**: Se o desenvolvedor concatenar valores em SQL
   strings, um atacante pode injetar comandos arbitrarios. A unica
   defesa confiavel e forcar parametrizacao por arquitetura — nao por
   convencao.

2. **Filtros obrigatorios esquecidos**: Em um sistema multi-tenant com
   concorrencia otimista, toda query deve filtrar por `TenantCode` e
   toda atualizacao deve verificar `EntityVersion`. Se um unico
   repositorio esquecer o filtro de tenant, um cliente acessa dados de
   outro. Se esquecer o version check, atualizacoes concorrentes causam
   lost updates silenciosos.

O `DataModelMapperBase` resolve ambos os riscos de forma estrutural:

- **Parametrizacao forcada**: A API do Mapper so aceita expression trees
  e valores tipados — nao ha como passar SQL cru. SQL injection se torna
  arquiteturalmente impossivel.
- **Filtros base embutidos**: `DataModelMapperBase` injeta
  automaticamente filtro por `TenantCode` em SELECTs e DELETEs, e
  `EntityVersion` check em UPDATEs e DELETEs. O desenvolvedor nao
  precisa lembrar — o framework garante.
- **Mapeamento type-safe**: Expression trees validam nomes de
  propriedades em compile-time.
- **Cache de clauses**: WHERE e ORDER BY sao cacheados na
  inicializacao — zero alocacao em runtime.
- **Bulk insert nativo**: COPY protocol do PostgreSQL via binary
  import.

## Como Normalmente E Feito

### Abordagem Tradicional

SQL manual nos repositorios com parametrizacao por convencao:

```csharp
public class UserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
    {
        // Desenvolvedor A: parametriza corretamente
        var cmd = new NpgsqlCommand(
            "SELECT id, email FROM auth_users " +
            "WHERE email = @email AND tenant_code = @tenant",
            _connection);
        cmd.Parameters.AddWithValue("email", email);
        cmd.Parameters.AddWithValue("tenant", tenantCode);
    }

    public async Task<List<User>> SearchAsync(string term)
    {
        // Desenvolvedor B: esquece de parametrizar — SQL INJECTION
        var cmd = new NpgsqlCommand(
            $"SELECT * FROM auth_users WHERE username LIKE '%{term}%'",
            _connection);
        // Sem filtro de tenant — ACESSA DADOS DE TODOS OS CLIENTES
    }

    public async Task<bool> UpdateAsync(User user)
    {
        // Desenvolvedor C: esquece version check — LOST UPDATES
        var cmd = new NpgsqlCommand(
            "UPDATE auth_users SET email = @email WHERE id = @id",
            _connection);
        // Sem EntityVersion check — concorrencia quebrada
        // Sem filtro de tenant — atualiza registro de outro cliente
    }
}
```

### Por Que Nao Funciona Bem

- **SQL injection por descuido**: Basta um desenvolvedor usar string
  interpolation em vez de parametros e o sistema fica vulneravel. Erros
  de digitacao em colunas sao pegos por testes de integracao, mas SQL
  injection pode passar despercebido se nao houver teste especifico.
- **Filtro de tenant esquecido**: Sem framework forcando, cada query
  depende do desenvolvedor lembrar de filtrar por `TenantCode`. Um
  unico esquecimento expoe dados de outros clientes.
- **Version check esquecido**: UPDATE e DELETE sem `EntityVersion`
  causam lost updates silenciosos — dois processos concorrentes
  sobrescrevem dados um do outro.
- **Campos de auditoria inconsistentes**: Sem padrao, `LastChangedBy`,
  `LastChangedAt`, `LastChangedCorrelationId` sao preenchidos de forma
  diferente (ou esquecidos) em cada repositorio.

## A Decisao

### Nossa Abordagem

Cada DataModel deve ter um Mapper dedicado:

```csharp
// ShopDemo.Auth.Infra.Data.PostgreSql/Mappers/UserDataModelMapper.cs
public sealed class UserDataModelMapper
    : DataModelMapperBase<UserDataModel>
{
    protected override void ConfigureInternal(
        MapperOptions<UserDataModel> mapperOptions)
    {
        mapperOptions
            .MapTable(schema: "public", name: "auth_users")
            .MapColumn(static x => x.Username)
            .MapColumn(static x => x.Email)
            .MapColumn(static x => x.PasswordHash)
            .MapColumn(static x => x.Status);
    }

    public override void MapBinaryImporter(
        NpgsqlBinaryImporter importer,
        UserDataModel model)
    {
        // Campos base (DataModelBase)
        importer.Write(model.Id, NpgsqlDbType.Uuid);
        importer.Write(model.TenantCode, NpgsqlDbType.Uuid);
        importer.Write(model.CreatedBy, NpgsqlDbType.Varchar);
        importer.Write(model.CreatedAt, NpgsqlDbType.TimestampTz);
        // ... demais campos base

        // Campos especificos
        importer.Write(model.Username, NpgsqlDbType.Varchar);
        importer.Write(model.Email, NpgsqlDbType.Varchar);
        importer.Write(model.PasswordHash, NpgsqlDbType.Bytea);
        importer.Write(model.Status, NpgsqlDbType.Smallint);
    }
}
```

**Como o Mapper e usado (SQL type-safe):**

```csharp
// No DataModelRepository — zero strings manuais
WhereClause whereClause =
    _mapper.Where(static (UserDataModel x) => x.Email)      // type-safe
    & _mapper.Where(static (UserDataModel x) => x.TenantCode);

string sql = _mapper.GenerateSelectCommand(whereClause);     // cacheado

await using NpgsqlCommand command = _unitOfWork.CreateNpgsqlCommand(sql);
_mapper.AddParameterForCommand(command,
    static (UserDataModel x) => x.Email, email);             // tipo correto
_mapper.AddParameterForCommand(command,
    static (UserDataModel x) => x.TenantCode, tenantCode);
```

**Regras fundamentais:**

1. **`sealed`**: O mapper nao pode ser estendido.
2. **Herda `DataModelMapperBase<TDataModel>`**: Herda geracao de SQL,
   cache de clauses, mapeamento automatico de campos base.
3. **`ConfigureInternal`**: Mapeia tabela e colunas especificas via
   expression trees.
4. **`MapBinaryImporter`**: Define a ordem de colunas para bulk insert.
5. **Namespace canonico**: `*.Mappers`.
6. **Nomenclatura**: `{Entity}DataModelMapper`.
7. **Expression trees com `static` lambdas**: `static x => x.Email` —
   zero alocacao de closures.

### Por Que Funciona Melhor

- **SQL injection impossivel por arquitetura**: A API do Mapper nao
  aceita SQL strings — so expression trees e valores tipados. Nao e
  possivel concatenar input do usuario em SQL porque a interface nao
  permite. `RelationalOperator` usa whitelist (`=`, `>`, `LIKE`);
  parametros sao tipados e escapados pelo Npgsql.
- **Filtro de tenant garantido**: `DataModelMapperBase` injeta
  `TenantCode` em toda query automaticamente. O desenvolvedor nao
  pode esquecer porque o framework faz por ele.
- **Concorrencia otimista garantida**: UPDATE e DELETE incluem
  `EntityVersion` check automaticamente via base class. Lost updates
  sao impossiveis.
- **Campos de auditoria padronizados**: `LastChangedBy`,
  `LastChangedAt`, `LastChangedCorrelationId` sao gerenciados pela
  base — nunca esquecidos, nunca inconsistentes.
- **Type safety**: Expression trees validam nomes de propriedades em
  compile-time.
- **Zero alocacao**: WHERE e ORDER BY clauses cacheados na
  inicializacao.
- **Bulk insert nativo**: `MapBinaryImporter` suporta COPY protocol
  do PostgreSQL para insercoes em massa.

## Consequencias

### Beneficios

- Geracao de SQL type-safe sem strings manuais.
- Zero alocacao apos inicializacao (clauses cacheados).
- Bulk insert via COPY protocol disponivel para todos os DataModels.
- Code agents geram Mappers corretos com 3 informacoes: schema, tabela
  e colunas especificas.

### Trade-offs (Com Perspectiva)

- **Mais uma classe por DataModel**: Na pratica, o Mapper e uma classe
  de configuracao com 10-20 linhas de mapeamento.
- **Binary import manual**: `MapBinaryImporter` lista colunas
  manualmente. A ordem deve corresponder exatamente ao schema do banco.
  Erros sao detectados em testes de integracao.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Data Mapper** (Fowler, POEAA): O Mapper implementa o padrao Data
  Mapper com geracao automatica de SQL.
- **Template Method** (GoF): `DataModelMapperBase` define o algoritmo
  de mapeamento; a classe concreta preenche os detalhes especificos.
- **Builder Pattern** (GoF): `MapperOptions` usa fluent builder para
  configuracao de tabela e colunas.

### O Que o Clean Code Diz

> "Don't Repeat Yourself."
>
> *Nao se repita.*

Hunt & Thomas (1999). O Mapper centraliza o mapeamento propriedade-coluna
em um unico ponto — eliminando duplicacao de SQL strings espalhadas por
repositorios.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Como o DataModelMapperBase gera SQL a partir de expression trees?"
2. "O que e o WhereClause e como ele previne SQL injection?"
3. "Como o MapBinaryImporter funciona para bulk insert?"

### Leitura Recomendada

- Martin Fowler, *Patterns of Enterprise Application Architecture*
  (2002), Cap. 10 — Data Mapper
- GoF, *Design Patterns* (1994) — Template Method, Builder

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Persistence.PostgreSql | Define `DataModelMapperBase<T>`, `WhereClause`, `OrderByClause`, `ColumnMap`, `MapperOptions` |

## Referencias no Codigo

- Mapper de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/Mappers/UserDataModelMapper.cs`
- Base class: `src/BuildingBlocks/Persistence.PostgreSql/Mappers/DataModelMapperBase.cs`
- ADR relacionada: [IN-010 — DataModel Herda DataModelBase](../infrastructure/IN-010-datamodel-herda-datamodelbase.md)
- ADR relacionada: [IN-011 — DataModelRepository Implementa Base](../infrastructure/IN-011-datamodel-repository-implementa-idatamodelrepository.md)
