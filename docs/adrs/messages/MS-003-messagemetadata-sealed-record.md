# MS-003: MessageMetadata Sealed Record Sem Herança

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine o formato de um envelope de carta padronizado pelos Correios: tamanho fixo, campos em posições definidas (remetente canto superior esquerdo, destinatário centro, selo canto superior direito). Se cada agência pudesse criar seu próprio formato de envelope, máquinas de triagem não funcionariam.

### O Problema Técnico

O envelope de mensagens precisa ser:
- **Padronizado**: Todo consumer espera os mesmos campos na mesma estrutura
- **Portátil**: Sem dependências de domain models — apenas primitivos serializáveis
- **Estável**: A estrutura não muda por tipo de mensagem

Se o envelope fosse herdável, subclasses poderiam adicionar campos, sobrescrever comportamento, ou introduzir tipos de domínio — quebrando a deserialização genérica.

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos frameworks usam classes abertas para metadata, ou até interfaces com múltiplas implementações:

```csharp
// Classe aberta — qualquer um pode herdar
public class MessageMetadata
{
    public Guid MessageId { get; set; }
    public string SchemaName { get; set; }
    // ...
}

// Alguém decide "estender"
public class ExtendedMetadata : MessageMetadata
{
    public Id DomainId { get; set; }  // tipo de domínio no envelope!
}
```

### Por Que Não Funciona Bem

- **Deserialização quebra**: Consumer espera `MessageMetadata`, recebe `ExtendedMetadata` — campos extras perdidos ou erro
- **Tipos de domínio no envelope**: `Id`, `BirthDate` acoplam consumidores ao domínio do produtor
- **Mutabilidade**: Setters públicos permitem alterar metadata após construção — compromete auditoria

## A Decisão

### Nossa Abordagem

`MessageMetadata` é um `sealed record` com apenas tipos primitivos:

```csharp
public sealed record MessageMetadata(
    Guid MessageId,
    DateTimeOffset Timestamp,
    string SchemaName,
    Guid CorrelationId,
    Guid TenantCode,
    string ExecutionUser,
    string ExecutionOrigin,
    string BusinessOperationCode
);
```

Características:
- **`sealed`**: Ninguém pode herdar e adicionar campos proprietários
- **`record`**: Imutável após construção, igualdade por valor, `ToString()` automático
- **Primitivos apenas**: `Guid`, `DateTimeOffset`, `string` — sem `Id`, `BirthDate`, `EntityInfo`

### Por Que Funciona Melhor

1. **Deserialização universal**: Qualquer consumer (C#, Python, Go) deserializa com os mesmos campos
2. **Imutabilidade**: Record é imutável — metadata não muda após construção
3. **Sem surpresas**: Tipo selado garante que o consumer sabe exatamente a estrutura
4. **Extensão centralizada**: Novos campos são adicionados AQUI — todos os tipos de mensagem ganham automaticamente

## Consequências

### Benefícios

- **Portabilidade**: Consumidores em qualquer linguagem deserializam sem referenciar domain models
- **Estabilidade**: Tipo selado — sem herança acidental
- **Evolução controlada**: Novos campos adicionados uma vez, propagados para todas as mensagens

### Trade-offs (Com Perspectiva)

- **Sem metadata customizada por tipo**: Um Command não pode ter campos extras no envelope
  - Campos específicos pertencem ao payload, não ao envelope. Isso é intencional
- **Todos os campos são obrigatórios**: Mesmo queries de leitura carregam `TenantCode`, `ExecutionUser`, etc.
  - O custo de serializar 8 campos primitivos é desprezível. A uniformidade simplifica infraestrutura de roteamento

## Fundamentação Teórica

### Padrões de Design Relacionados

**Value Object (DDD)**: `MessageMetadata` é um Value Object — sem identidade própria, definido pelo valor de seus campos, imutável. Dois metadatas com os mesmos campos são considerados iguais.

**Canonical Data Model (EIP)**: O envelope segue um formato canônico que todos os participantes do sistema entendem, independente do tipo de mensagem ou bounded context.

> "Define a Canonical Data Model that is independent from any specific application."
>
> *Defina um Modelo de Dados Canônico que é independente de qualquer aplicação específica.*

### O Que o Clean Architecture Diz

Tipos que cruzam fronteiras de processo devem ser os mais simples e portáteis possíveis. Primitivos (`Guid`, `string`, `DateTimeOffset`) não criam dependências entre camadas. Tipos de domínio (`Id`, `BirthDate`) forçariam consumidores a referenciar BuildingBlocks.Core — violando a regra de dependência.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Por que Value Objects devem ser imutáveis?"
- "Quais são os riscos de usar tipos de domínio em contratos de mensageria?"
- "Como o Canonical Data Model Pattern se aplica a microsserviços?"

### Leitura Recomendada

- [Enterprise Integration Patterns - Canonical Data Model](https://www.enterpriseintegrationpatterns.com/patterns/messaging/CanonicalDataModel.html)
- [Domain-Driven Design - Value Objects](https://martinfowler.com/bliki/ValueObject.html)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Messages | MessageMetadata implementa este padrão como sealed record com primitivos |

## Referências no Código

- [MessageMetadata.cs](../../../src/BuildingBlocks/Messages/MessageMetadata.cs) - declaração `public sealed record MessageMetadata`
