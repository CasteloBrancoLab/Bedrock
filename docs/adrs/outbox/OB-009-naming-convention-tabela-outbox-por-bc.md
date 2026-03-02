# OB-009: Naming Convention de Tabelas Outbox por BC

## Status

Aceita

## Contexto

### O Problema (Analogia)

Num condominio, cada apartamento tem a sua propria caixa de correio,
etiquetada com o numero do apartamento. Se todas as caixas se chamassem
"caixa de correio" sem distincao, o carteiro nao saberia onde colocar
cada carta — e os moradores receberiam correio alheio.

### O Problema Tecnico

Num monorepo com multiplos bounded contexts, cada BC tem o seu proprio
outbox. Se todos usarem a mesma tabela `outbox`:

1. **Conflito de dados**: Entries de BCs diferentes misturam-se.
2. **Lock contention**: `FOR UPDATE SKIP LOCKED` de um BC bloqueia
   (ou salta) entries de outro — reduz throughput.
3. **Ambiguidade operacional**: Queries de monitorizacao nao distinguem
   facilmente entre BCs.
4. **Migracoes acopladas**: Alteracao na tabela partilhada afecta todos
   os BCs.

Alternativas como schema-per-BC (`auth.outbox`, `catalog.outbox`)
resolvem parcialmente, mas adicionam complexidade de connection strings
e permissoes. A abordagem mais simples e um **prefixo no nome da tabela**.

## A Decisao

Cada BC configura o nome da tabela outbox com o prefixo do bounded
context, usando o padrao `{bc_prefix}_outbox`:

```csharp
// AuthOutboxRepository
protected override void ConfigureInternal(OutboxPostgreSqlOptions options)
{
    options.WithTableName("auth_outbox");
    // Schema defaults to "public"
}

// (futuro) CatalogOutboxRepository
protected override void ConfigureInternal(OutboxPostgreSqlOptions options)
{
    options.WithTableName("catalog_outbox");
}
```

A migration correspondente:

```sql
CREATE TABLE auth_outbox (
    id UUID PRIMARY KEY,
    -- ... campos do outbox
);
```

**Regras fundamentais:**

1. **Padrao**: `{bc_prefix}_outbox` — ex: `auth_outbox`,
   `catalog_outbox`, `billing_outbox`.
2. **Prefixo = abreviacao do BC**: Mesmo prefixo usado em outras
   tabelas do BC (ex: `auth_users`, `auth_roles`).
3. **Schema `public`**: Todos os BCs partilham o schema `public`
   (decisao existente do projecto). A distincao e pelo prefixo.
4. **ConfigureInternal**: O BC define via
   `options.WithTableName("...")` — a base constroi o SQL.
5. **Um ficheiro de migration por BC**: Cada BC tem o seu proprio
   script de criacao da tabela outbox.

## Consequencias

### Beneficios

- Zero conflito entre BCs — cada um tem tabela dedicada.
- Workers podem processar tabelas especificas sem interferencia.
- Queries operacionais facilmente filtradas por tabela.
- Migracoes independentes por BC.
- Consistencia com naming convention existente do projecto.

### Trade-offs (Com Perspectiva)

- **Mais tabelas no schema**: N BCs = N tabelas outbox. Na pratica,
  o custo para o PostgreSQL de tabelas adicionais e negligivel.
- **Prefixo manual**: O desenvolvedor deve lembrar-se de usar o
  prefixo correto. O padrao `ConfigureInternal` + code review/arch
  rules mitiga erros.

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Outbox.PostgreSql | `OutboxPostgreSqlOptions.WithTableName()` |

## Referencias no Codigo

- Opcoes: `src/BuildingBlocks/Outbox.PostgreSql/OutboxPostgreSqlOptions.cs`
- Exemplo Auth: `src/ShopDemo/Auth/Infra.Data.PostgreSql/Outbox/AuthOutboxRepository.cs`
- Migration Auth: `src/ShopDemo/Auth/Infra.Data.PostgreSql.Migrations/Scripts/Up/V202603010001__create_auth_outbox_table.sql`
