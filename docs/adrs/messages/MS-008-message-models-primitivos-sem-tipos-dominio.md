# MS-008: Message Models com Primitivos — Sem Tipos de Domínio

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine que você envia um pacote internacional. O formulário da alfândega exige: peso em kg, valor em USD, descrição em texto. Se você escrevesse "3 arrobas" (unidade portuguesa antiga) no peso, a alfândega de Tokyo não entenderia. Unidades internacionais (primitivos) garantem que qualquer país processa o formulário.

### O Problema Técnico

Mensagens cruzam fronteiras de processo — entre microserviços, linguagens, e bounded contexts. Se o payload usa tipos de domínio (Value Objects como `Id`, `BirthDate`, `EntityInfo`), consumidores em outros BCs ou linguagens (Python, Go) precisariam:
- Referenciar o assembly `Bedrock.BuildingBlocks.Core`
- Entender a semântica de `Id.Value`, `BirthDate.Value`
- Implementar deserialização customizada para esses tipos

Além disso, se o schema do Value Object mudar (ex: `BirthDate` ganha um campo `TimeZone`), todas as mensagens que usam `BirthDate` quebram — mesmo que o campo `TimeZone` seja irrelevante para a mensagem.

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos projetos reusam os domain models diretamente nas mensagens:

```csharp
// Domain model como payload da mensagem
public record UserRegisteredEvent(
    Id UserId,           // Value Object do domínio
    Email EmailAddress,  // Outro Value Object
    BirthDate BirthDate  // Mais um
);
```

### Por Que Não Funciona Bem

- **Acoplamento cross-BC**: Consumidor precisa de `Bedrock.BuildingBlocks.Core` para deserializar `Id`
- **Acoplamento cross-linguagem**: Consumer em Python não tem `BirthDate`
- **Schema frágil**: `BirthDate` ganha `TimeZone` → mensagem quebra em todos os consumers
- **Versioning impossível**: Não é possível ter V1 e V2 do mesmo Value Object no mesmo assembly

## A Decisão

### Nossa Abordagem

Message Models usam APENAS tipos primitivos serializáveis (`Guid`, `string`, `DateTimeOffset`, `int`, `decimal`). São `readonly record struct` para zero alocação heap:

```csharp
// Message Model — apenas primitivos
[ExcludeFromCodeCoverage(Justification = "Readonly record struct — ...")]
public readonly record struct SimpleAggregateRootModel(
    Guid Id,                    // não Id (Value Object)
    Guid TenantCode,            // não TenantInfo
    string FirstName,
    string LastName,
    string FullName,
    DateTimeOffset BirthDate,   // não BirthDate (Value Object)
    DateTimeOffset CreatedAt,   // não EntityChangeInfo
    string CreatedBy,
    DateTimeOffset? LastModifiedAt,
    string? LastModifiedBy
);
```

Comparação com o domínio:

| Domínio | Message Model |
|---------|---------------|
| `Id` (Value Object) | `Guid` |
| `BirthDate` (Value Object) | `DateTimeOffset` |
| `EntityInfo.Id` | `Guid Id` |
| `EntityInfo.TenantInfo.TenantCode` | `Guid TenantCode` |
| `EntityInfo.EntityChangeInfo.CreatedAt` | `DateTimeOffset CreatedAt` |

### Por Que Funciona Melhor

1. **Zero dependências**: Consumer não precisa de nenhum assembly do domínio
2. **Cross-linguagem**: JSON com `Guid`, `string`, `DateTimeOffset` é universal
3. **Schema isolado**: Value Object muda → Message Model não muda (são independentes)
4. **Versionamento**: V1 e V2 do model podem coexistir com estruturas diferentes

## Consequências

### Benefícios

- **Portabilidade**: Qualquer linguagem/framework deserializa primitivos
- **Isolamento de schema**: Schema da mensagem evolui independente do domínio
- **Performance**: `readonly record struct` é stack-allocated — zero alocação heap
- **Cobertura**: `ExcludeFromCodeCoverage` com justificativa evita falsos negativos do Coverlet

### Trade-offs (Com Perspectiva)

- **Mapeamento necessário**: Produtor precisa mapear de domain model para message model
  - Esse mapeamento é explícito e testável. É preferível a acoplamento implícito
- **Duplicação aparente**: Message Model parece duplicar o domain model
  - São schemas independentes com ciclos de vida diferentes. "Duplicação" aqui é desacoplamento intencional
- **Perda de semântica**: `Guid` não carrega a semântica de `Id` (não pode ser vazio, etc.)
  - A validação pertence ao domínio, não à mensagem. A mensagem é um DTO de fronteira

## Fundamentação Teórica

### Padrões de Design Relacionados

**Data Transfer Object (DTO)**: Message Models são DTOs — objetos que carregam dados entre processos sem comportamento. Usar primitivos é a essência do DTO: máxima portabilidade, mínimo acoplamento.

**Anti-Corruption Layer (DDD)**: O mapeamento entre domain model e message model é uma Anti-Corruption Layer. Protege o domínio de mudanças no schema da mensagem e vice-versa.

> "Create an isolating layer to provide clients with functionality in terms of their own domain model."
>
> *Crie uma camada de isolamento para fornecer aos clientes funcionalidade nos termos de seu próprio domain model.*

### O Que o Clean Architecture Diz

Clean Architecture coloca DTOs na fronteira (Interface Adapters layer). Eles traduzem entre a representação interna (domain models) e a representação externa (mensagens). Usar primitivos na fronteira garante que a camada externa não polui a interna.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Por que DTOs devem usar tipos primitivos em vez de Value Objects?"
- "Como Anti-Corruption Layer se aplica a mensageria?"
- "Quais são os riscos de acoplar schema de mensagem ao domain model?"

### Leitura Recomendada

- [Anti-Corruption Layer - Martin Fowler](https://martinfowler.com/bliki/AntiCorruptionLayer.html)
- [DTO Pattern - Microsoft Docs](https://learn.microsoft.com/en-us/aspnet/web-api/overview/data/using-web-api-with-entity-framework/part-5)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Messages | MessageMetadata segue a mesma regra (Guid, não Id) |

## Referências no Código

- [SimpleAggregateRootModel.cs](../../../src/Templates/Infra.CrossCutting.Messages/V1/Models/SimpleAggregateRootModel.cs) - snapshot com primitivos
- [RegisterSimpleAggregateRootInputModel.cs](../../../src/Templates/Infra.CrossCutting.Messages/V1/Models/RegisterSimpleAggregateRootInputModel.cs) - input com primitivos
- [MessageMetadata.cs](../../../src/BuildingBlocks/Messages/MessageMetadata.cs) - `Guid MessageId` em vez de `Id MessageId`
