# RL-004: DataModel Deve Ter Apenas Propriedades Primitivas

## Status

Aceita

## Validacao Automatizada

Esta ADR sera validada pela rule de arquitetura
**RL004_DataModelOnlyPrimitivePropertiesRule**, que verifica:

- Para cada DataModel (classe em `*.DataModels` com sufixo `DataModel`),
  todas as propriedades declaradas devem ser de tipos primitivos.
- Tipos permitidos: `string`, `int`, `long`, `short`, `byte`, `bool`,
  `float`, `double`, `decimal`, `char`, `Guid`, `DateTime`,
  `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan`, `byte[]`,
  e suas versoes nullable.
- Enums sao permitidos (mapeados como inteiros no banco).
- Value objects, entidades, listas e outros tipos complexos sao proibidos.

## Contexto

### O Problema (Analogia)

Imagine um formulario de importacao de dados que aceita apenas campos
preenchidos a mao — nome, CPF, data de nascimento. Se alguem tentar
colar uma foto ou um PDF no campo "nome", o formulario rejeita. Cada
campo aceita exatamente o tipo de dado que o banco de dados consegue
armazenar.

### O Problema Tecnico

DataModels sao DTOs (Data Transfer Objects) planos que representam
exatamente o schema da tabela no banco relacional. Se um DataModel
contiver propriedades complexas (value objects, entidades, listas):

1. **Mapeamento impossivel**: O `DataModelMapperBase` nao sabe como
   mapear um `Address` para colunas SQL. Ele espera tipos que o
   Npgsql consegue serializar diretamente.
2. **Confusao de responsabilidade**: O DataModel nao deve conter
   logica de dominio. Value objects e entidades pertencem ao Domain
   — nao ao Data layer.
3. **Bulk insert quebrado**: O `MapBinaryImporter` escreve colunas
   sequencialmente via `NpgsqlBinaryImporter.Write()`. Tipos complexos
   nao sao suportados pelo protocolo COPY binario.

## A Decisao

DataModels devem conter apenas propriedades de tipos primitivos:

```csharp
// CORRETO
public class UserDataModel : DataModelBase
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public byte[] PasswordHash { get; set; } = null!;
    public short Status { get; set; }  // Enum mapeado como short
}

// ERRADO
public class UserDataModel : DataModelBase
{
    public string Username { get; set; } = null!;
    public EmailAddress Email { get; set; }     // Value object!
    public List<Role> Roles { get; set; }       // Colecao!
    public Address HomeAddress { get; set; }    // Tipo complexo!
}
```

### Por Que Funciona

- **Compatibilidade com Npgsql**: Todos os tipos permitidos sao
  diretamente serializaveis pelo Npgsql (parametros e COPY binario).
- **Schema 1:1**: Cada propriedade do DataModel corresponde exatamente
  a uma coluna da tabela.
- **Separacao clara**: Conversao entre dominio e dados acontece no
  Adapter/Factory — nao no DataModel.

## Consequencias

### Beneficios

- DataModels sao DTOs puros sem logica.
- Mapeamento objeto-relacional sempre funciona.
- Bulk insert via COPY protocol garantido para todos os DataModels.
- Code agents geram DataModels corretos com o schema da tabela.

### Trade-offs

- **Conversao explicita**: Tipos complexos do dominio devem ser
  convertidos para primitivos no Adapter (ex: `EmailAddress` → `string`).
  Na pratica, isso e uma unica linha de codigo por propriedade.

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Persistence.PostgreSql | Define `DataModelBase` com propriedades primitivas |

## Referencias no Codigo

- DataModel de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/DataModels/UserDataModel.cs`
- ADR relacionada: [IN-010 — DataModel Herda DataModelBase](../infrastructure/IN-010-datamodel-herda-datamodelbase.md)
- ADR relacionada: [RL-001 — Mapper Herda DataModelMapperBase](./RL-001-mapper-herda-datamodelmapperbase.md)
