# IN-013: Factories Bidirecionais Para Conversao DataModel e Entidade

## Status

Aceita

## Validacao Automatizada

Esta ADR sera validada pela rule de arquitetura
**IN013_BidirectionalFactoriesRule**, que verifica:

- Para cada DataModel em `*.DataModels`, devem existir duas static
  classes no namespace `*.Factories`:
  - `{Entity}Factory` — converte DataModel para aggregate root.
  - `{Entity}DataModelFactory` — converte aggregate root para DataModel.
- Ambas devem ser `static class`.
- Ambas devem possuir metodo publico `Create`.

## Contexto

### O Problema (Analogia)

Imagine uma embaixada que processa vistos. Quando um documento chega do
pais de origem, precisa ser traduzido para o idioma local. Quando um
documento e emitido localmente, precisa ser traduzido para o idioma do
pais de destino. A embaixada precisa de dois tradutores: um para cada
direcao. Se so tiver um tradutor (ex: so traduz para o idioma local), os
documentos emitidos localmente nao podem ser enviados de volta.

### O Problema Tecnico

A separacao entre entidades de dominio e DataModels
([IN-004](./IN-004-modelo-dados-detalhe-implementacao.md)) exige
conversao bidirecional:

- **Leitura (DataModel → Entity)**: Ao buscar do banco, o DataModel
  precisa ser convertido em aggregate root com todos os seus value
  objects reconstruidos.
- **Escrita (Entity → DataModel)**: Ao persistir, o aggregate root
  precisa ser convertido em DataModel com tipos primitivos compativeis
  com o banco.

Se a conversao nao for centralizada em factories dedicadas, o codigo
de conversao fica espalhado por repositorios, services e handlers —
cada um fazendo a conversao de um jeito diferente, com riscos de
campos esquecidos ou value objects mal reconstruidos.

## Como Normalmente E Feito

### Abordagem Tradicional

A conversao fica inline nos repositorios:

```csharp
public class UserRepository
{
    public async Task<User?> GetByIdAsync(Guid id)
    {
        var reader = await ExecuteQuery("SELECT ...");
        // Conversao inline — espalhada por cada metodo
        return new User(
            reader.GetGuid("id"),
            reader.GetString("email"),
            // Esqueceu PasswordHash? Esqueceu TenantCode?
        );
    }

    public async Task InsertAsync(User user)
    {
        var cmd = new NpgsqlCommand("INSERT ...");
        // Conversao inline — duplicada em cada metodo
        cmd.Parameters.Add("email", user.Email.Value);
        cmd.Parameters.Add("password_hash", user.PasswordHash.Value);
        // Esqueceu EntityVersion? Esqueceu CreatedCorrelationId?
    }
}
```

### Por Que Nao Funciona Bem

- **Conversao espalhada**: Cada metodo do repositorio reimplementa a
  conversao — nao ha ponto unico de verdade.
- **Campos esquecidos**: Sem uma factory centralizada, e facil esquecer
  um campo de auditoria ou um value object.
- **Inconsistencia entre direcoes**: A conversao de leitura pode
  reconstruir o `PasswordHash` corretamente, mas a de escrita pode
  esquece-lo — ou vice-versa.
- **Dificil de testar**: Testar a conversao exige testar cada metodo do
  repositorio individualmente.

## A Decisao

### Nossa Abordagem

Cada aggregate root persistido deve ter duas factories:

**1. Entity Factory (DataModel → Entity):**

```csharp
// ShopDemo.Auth.Infra.Data.PostgreSql/Factories/UserFactory.cs
public static class UserFactory
{
    public static User Create(UserDataModel dataModel)
    {
        // Reconstroi EntityInfo completo
        EntityInfo entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(dataModel.Id),
            tenantInfo: TenantInfo.Create(dataModel.TenantCode),
            createdAt: dataModel.CreatedAt,
            createdBy: dataModel.CreatedBy,
            createdCorrelationId: dataModel.CreatedCorrelationId,
            // ... todos os campos de auditoria
            entityVersion: RegistryVersion.CreateFromExistingInfo(
                dataModel.EntityVersion));

        // Reconstroi aggregate com value objects
        return User.CreateFromExistingInfo(
            new CreateFromExistingInfoInput(
                entityInfo,
                dataModel.Username,
                EmailAddress.CreateNew(dataModel.Email),
                PasswordHash.CreateNew(dataModel.PasswordHash),
                (UserStatus)dataModel.Status));
    }
}
```

**2. DataModel Factory (Entity → DataModel):**

```csharp
// ShopDemo.Auth.Infra.Data.PostgreSql/Factories/UserDataModelFactory.cs
public static class UserDataModelFactory
{
    public static UserDataModel Create(User entity)
    {
        // Usa DataModelBaseFactory para campos de auditoria
        UserDataModel dataModel =
            DataModelBaseFactory.Create<UserDataModel, User>(entity);

        // Mapeia campos especificos com tipos primitivos
        dataModel.Username = entity.Username;
        dataModel.Email = entity.Email.Value;        // Value object → string
        dataModel.PasswordHash = entity.PasswordHash
            .Value.ToArray();                        // ReadOnlyMemory → byte[]
        dataModel.Status = (short)entity.Status;     // Enum → short

        return dataModel;
    }
}
```

**Regras fundamentais:**

1. **Duas factories por aggregate root**: `{Entity}Factory` e
   `{Entity}DataModelFactory`.
2. **Static classes**: Sem estado, sem dependencias, puras funcoes de
   conversao.
3. **Metodo `Create`**: Ponto unico de conversao para cada direcao.
4. **Namespace canonico**: `*.Factories`.
5. **Entity Factory reconstroi value objects**: `EmailAddress.CreateNew`,
   `PasswordHash.CreateNew` — nao passa strings cruas.
6. **DataModel Factory extrai primitivos**: `.Value`, `.ToArray()`,
   `(short)` — converte value objects para tipos do banco.
7. **`DataModelBaseFactory` para campos de auditoria**: Nunca copiar
   campos base manualmente — usar a factory do framework.

### Por Que Funciona Melhor

- **Ponto unico de conversao**: Toda leitura passa pela Entity Factory,
  toda escrita pela DataModel Factory. Se um campo mudar, corrige em
  um lugar so.
- **Value objects reconstruidos corretamente**: A Entity Factory garante
  que `EmailAddress`, `PasswordHash` etc. sao recriados com validacao.
- **Campos de auditoria nunca esquecidos**: `DataModelBaseFactory`
  copia os 13 campos de `DataModelBase` automaticamente.
- **Testavel isoladamente**: Cada factory pode ser testada com dados
  fake sem precisar de banco.

## Consequencias

### Beneficios

- Conversao bidirecional centralizada e testavel.
- Value objects do dominio reconstruidos com validacao.
- Campos de auditoria copiados via framework — zero esquecimento.
- Code agents geram factories corretas seguindo o template.

### Trade-offs (Com Perspectiva)

- **Duas classes por aggregate root**: Na pratica, factories sao classes
  pequenas (10-20 linhas) com logica simples de mapeamento.
- **Sincronizacao manual**: Se a entidade ganha uma nova propriedade,
  as factories precisam ser atualizadas. Os testes de mutacao detectam
  propriedades nao mapeadas (mutante sobrevive se o campo for ignorado).

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Factory Method** (GoF): Encapsula a logica de criacao de objetos.
- **Data Mapper** (Fowler, POEAA): Factories implementam o padrao Data
  Mapper — transferem dados entre objetos e banco mantendo ambos
  independentes.
- **Adapter Pattern** (GoF): Adapta a "interface" do DataModel para a
  "interface" da entidade e vice-versa.

### O Que o DDD Diz

> "Reconstituting an object from persistence is a Factory responsibility."
>
> *Reconstituir um objeto a partir da persistencia e responsabilidade de
> uma Factory.*

Evans (2003). A Entity Factory materializa essa responsabilidade:
reconstroi o aggregate root com todos os seus invariantes a partir de
dados brutos.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Por que usar static classes para factories de conversao?"
2. "Qual a diferenca entre UserFactory e UserDataModelFactory?"
3. "Como DataModelBaseFactory automatiza a copia de campos de auditoria?"

### Leitura Recomendada

- GoF, *Design Patterns* (1994) — Factory Method
- Martin Fowler, *Patterns of Enterprise Application Architecture*
  (2002), Cap. 10 — Data Mapper
- Eric Evans, *Domain-Driven Design* (2003), Cap. 6 — Factories

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Persistence.PostgreSql | Define `DataModelBaseFactory` — factory base que copia campos de auditoria automaticamente |
| Bedrock.BuildingBlocks.Domain.Entities | Define os tipos de dominio que as Entity Factories reconstroem |

## Referencias no Codigo

- Entity Factory de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/Factories/UserFactory.cs`
- DataModel Factory de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/Factories/UserDataModelFactory.cs`
- Factory base: `src/BuildingBlocks/Persistence.PostgreSql/Factories/DataModelBaseFactory.cs`
- ADR relacionada: [IN-004 — Modelo de Dados E Detalhe de Implementacao](./IN-004-modelo-dados-detalhe-implementacao.md)
- ADR relacionada: [IN-010 — DataModel Herda DataModelBase](./IN-010-datamodel-herda-datamodelbase.md)
