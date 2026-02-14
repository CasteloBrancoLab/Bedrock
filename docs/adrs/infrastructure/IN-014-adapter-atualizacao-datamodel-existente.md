# IN-014: Adapter Para Atualizacao de DataModel Existente

## Status

Aceita

## Validacao Automatizada

Esta ADR sera validada pela rule de arquitetura
**IN014_DataModelAdapterRule**, que verifica:

- Para cada DataModel em `*.DataModels`, deve existir uma static class
  no namespace `*.Adapters` com nome `{Entity}DataModelAdapter`.
- A classe deve possuir metodo publico `Adapt` que recebe o DataModel
  existente e a entidade de dominio, e retorna o DataModel atualizado.

## Contexto

### O Problema (Analogia)

Imagine um cartorio. Quando voce muda de endereco, nao cria um novo
registro de nascimento — atualiza o registro existente. O funcionario
do cartorio pega o registro original, altera os campos necessarios e
mantem tudo que nao mudou (nome, data de nascimento, filiacao). Criar
um registro novo perderia o historico de alteracoes e o numero original.

### O Problema Tecnico

No fluxo de `Update`, o repositorio precisa:

1. Buscar o DataModel existente do banco (com `EntityVersion` atual).
2. Atualizar os campos que mudaram com os valores do aggregate root.
3. Persistir o DataModel atualizado com version check.

A Factory (IN-013) cria DataModels **novos** a partir de aggregates.
Mas no Update, precisamos **modificar** um DataModel existente —
preservando campos que so existem no banco (ex: `CreatedAt`,
`CreatedCorrelationId`) e atualizando apenas os campos que mudaram.

Se a atualizacao for feita inline no repositorio, o mesmo problema
da IN-013 ocorre: codigo espalhado, campos esquecidos, inconsistencias.

## Como Normalmente E Feito

### Abordagem Tradicional

A atualizacao fica inline no metodo `Update`:

```csharp
public async Task<bool> UpdateAsync(User user)
{
    var existing = await GetDataModelById(user.Id);

    // Atualizacao manual — facil de esquecer campos
    existing.Email = user.Email.Value;
    existing.Username = user.Username;
    // Esqueceu PasswordHash? Esqueceu Status?
    // Esqueceu LastChangedBy? Esqueceu LastChangedCorrelationId?

    return await SaveAsync(existing);
}
```

### Por Que Nao Funciona Bem

- **Campos esquecidos**: Sem centralizacao, e facil omitir um campo
  na atualizacao — especialmente campos de auditoria.
- **Duplicacao**: Se ha varios metodos de update (full update, partial
  update), cada um reimplementa a copia de campos.
- **Sem testabilidade isolada**: A logica de adaptacao esta embutida
  no repositorio — nao pode ser testada separadamente.

## A Decisao

### Nossa Abordagem

Cada aggregate root persistido deve ter um Adapter dedicado:

```csharp
// ShopDemo.Auth.Infra.Data.PostgreSql/Adapters/UserDataModelAdapter.cs
public static class UserDataModelAdapter
{
    public static UserDataModel Adapt(
        UserDataModel dataModel,    // DataModel EXISTENTE do banco
        User entity                 // Aggregate root com valores atualizados
    )
    {
        // Adapta campos base (auditoria, versao)
        DataModelBaseAdapter.Adapt(dataModel, entity);

        // Adapta campos especificos
        dataModel.Username = entity.Username;
        dataModel.Email = entity.Email.Value;
        dataModel.PasswordHash = entity.PasswordHash.Value.ToArray();
        dataModel.Status = (short)entity.Status;

        return dataModel;
    }
}
```

**Uso no repositorio tecnologico:**

```csharp
public async Task<bool> UpdateAsync(
    ExecutionContext executionContext,
    User aggregateRoot,
    CancellationToken cancellationToken)
{
    // 1. Busca DataModel existente
    UserDataModel? existing = await _dataModelRepository.GetByIdAsync(
        executionContext, aggregateRoot.EntityInfo.Id, cancellationToken);

    if (existing is null) return false;

    // 2. Adapta — ponto unico de atualizacao
    UserDataModelAdapter.Adapt(existing, aggregateRoot);

    // 3. Persiste com version check
    return await _dataModelRepository.UpdateAsync(
        executionContext, existing,
        aggregateRoot.EntityInfo.EntityVersion, cancellationToken);
}
```

**Regras fundamentais:**

1. **Static class**: Sem estado, funcao pura de adaptacao.
2. **Metodo `Adapt`**: Recebe DataModel existente e entidade, retorna
   DataModel atualizado (mesmo objeto, modificado in-place).
3. **`DataModelBaseAdapter.Adapt` para campos base**: Nunca copiar
   campos de auditoria manualmente.
4. **Namespace canonico**: `*.Adapters`.
5. **Nomenclatura**: `{Entity}DataModelAdapter`.

**Diferenca entre Factory e Adapter:**

| Aspecto | Factory (IN-013) | Adapter (IN-014) |
|---------|-----------------|------------------|
| Operacao | **Cria** novo DataModel | **Modifica** DataModel existente |
| Quando usar | `Insert` (novo registro) | `Update` (registro existente) |
| Campos de criacao | Preenche `CreatedAt`, `CreatedBy` | **Preserva** `CreatedAt`, `CreatedBy` |
| Campos de alteracao | N/A | Preenche `LastChangedAt`, `LastChangedBy` |
| Retorno | Novo objeto | Mesmo objeto (modificado) |

### Por Que Funciona Melhor

- **Campos de auditoria corretos**: `DataModelBaseAdapter` atualiza
  `LastChangedAt`, `LastChangedBy`, etc. sem sobrescrever `CreatedAt`.
- **Ponto unico**: Todos os metodos de update usam o mesmo Adapter.
- **Testavel**: O Adapter pode ser testado isoladamente com dados fake.
- **Completa a bidirecionalidade**: Factory (leitura e criacao) +
  Adapter (atualizacao) cobrem todos os fluxos de persistencia.

## Consequencias

### Beneficios

- Atualizacao de DataModels centralizada e testavel.
- Campos de auditoria (LastChanged*) atualizados automaticamente via
  framework.
- Campos de criacao (Created*) preservados — nunca sobrescritos.
- Code agents geram Adapters corretos seguindo o template.

### Trade-offs (Com Perspectiva)

- **Mais uma classe por aggregate root**: Factory, DataModelFactory e
  agora Adapter. Na pratica, o Adapter e a mais simples das tres (5-10
  linhas de mapeamento).
- **Mutacao in-place**: O Adapter modifica o DataModel recebido em vez
  de criar um novo. Isso e intencional — preserva campos que so existem
  no DataModel original (campos de criacao).

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Adapter Pattern** (GoF): Adapta a "interface" da entidade de dominio
  para a "interface" do DataModel existente.
- **Data Mapper** (Fowler, POEAA): O Adapter e parte do Data Mapper —
  responsavel pela direcao Entity → DataModel no fluxo de update.

### O Que o DDD Diz

> "Reconstituting and persisting objects are infrastructure concerns."
>
> *Reconstituir e persistir objetos sao preocupacoes de infraestrutura.*

Evans (2003). O Adapter e infraestrutura pura — o dominio nunca sabe
que ele existe.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Qual a diferenca entre UserDataModelFactory e UserDataModelAdapter?"
2. "Por que o Adapter modifica in-place em vez de criar um novo
   DataModel?"
3. "Como DataModelBaseAdapter preserva campos de criacao e atualiza
   campos de alteracao?"

### Leitura Recomendada

- GoF, *Design Patterns* (1994) — Adapter Pattern
- Martin Fowler, *Patterns of Enterprise Application Architecture*
  (2002), Cap. 10 — Data Mapper

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Persistence.PostgreSql | Define `DataModelBaseAdapter` — adapter base que atualiza campos de auditoria |

## Referencias no Codigo

- Adapter de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/Adapters/UserDataModelAdapter.cs`
- Adapter base: `src/BuildingBlocks/Persistence.PostgreSql/Adapters/DataModelBaseAdapter.cs`
- ADR relacionada: [IN-013 — Factories Bidirecionais](./IN-013-factories-bidirecionais-datamodel-entidade.md)
- ADR relacionada: [IN-004 — Modelo de Dados E Detalhe de Implementacao](./IN-004-modelo-dados-detalhe-implementacao.md)
