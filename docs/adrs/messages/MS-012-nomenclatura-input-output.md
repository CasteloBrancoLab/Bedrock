# MS-012: Nomenclatura Input/Output para Models e Retornos

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma fábrica com duas esteiras: uma de entrada (matéria-prima) e uma de saída (produto acabado). Se alguém rotulasse a esteira de saída como "Resultado" em vez de "Saída", seria ambíguo — "resultado" pode significar sucesso/falha, score, ou o produto em si. "Saída" (output) é inequívoco: é o que sai do processo. Da mesma forma, "Input" é o que entra e "Output" é o que sai.

### O Problema Técnico

Em sistemas com mensagens de integração, existem dois tipos de dados auxiliares:

- **Input**: Dados que o chamador fornece para iniciar a operação (ex: `RegisterUserInputModel`)
- **Output**: Dados que a operação produz como resultado (ex: `DeactivationOutputModel`)

Se usarmos `*Result` ou `*ResultModel` em vez de `*Output`/`*OutputModel`, geramos ambiguidade:

- `Result` pode significar sucesso/falha (como `Result<T>` de railway programming)
- `Result` pode significar o retorno de uma query (como `SearchResult`)
- `Result` pode significar o output de uma operação

A mesma ambiguidade se aplica a namespaces: `*.Results` vs `*.Outputs`.

## Como Normalmente É Feito

### Abordagem Tradicional

Muitos projetos misturam terminologia sem convenção clara:

```csharp
// Mistura de sufixos — ambíguo
public readonly record struct DeactivationResultModel(...);  // É sucesso/falha? É output?
public readonly record struct UserSearchResult(...);          // É uma query? É um model?

// Namespace inconsistente
namespace Auth.Domain.Services.Results;  // "Results" de quê?
public readonly record struct UserDeactivationResult(...);
```

### Por Que Não Funciona Bem

- **Ambiguidade semântica**: `Result` tem múltiplos significados em software — sucesso/falha, retorno de query, output de operação
- **Conflito com patterns**: `Result<T>` é um pattern estabelecido para railway programming. Usar `Result` para output gera confusão
- **Inconsistência**: Sem convenção, cada desenvolvedor escolhe um sufixo diferente
- **Grep difícil**: Buscar por "Result" retorna falsos positivos de todo o codebase

## A Decisão

### Nossa Abordagem

Adotamos uma convenção clara e sem ambiguidade:

| Contexto | Sufixo | Namespace | Exemplo |
|----------|--------|-----------|---------|
| Dados de entrada em Messages | `*InputModel` | `*.Models` | `RegisterUserInputModel` |
| Dados de saída em Messages | `*OutputModel` | `*.Models` | `DeactivationOutputModel` |
| Retornos de Domain Services | `*Output` | `*.Outputs` | `UserDeactivationOutput` |

O que é proibido:

```csharp
// PROIBIDO em Messages: sufixo *ResultModel
public readonly record struct DeactivationResultModel(...);  // ← violação MS-012a

// PROIBIDO em Domain.Services: sufixo *Result ou namespace *.Results
namespace Auth.Domain.Services.Results;                      // ← violação MS-012b
public readonly record struct UserDeactivationResult(...);   // ← violação MS-012b
```

O que é correto:

```csharp
// CORRETO em Messages: sufixo *OutputModel
public readonly record struct DeactivationOutputModel(
    int RevokedRefreshTokenCount,
    int RevokedServiceClientCount,
    int RevokedApiKeyCount
);

// CORRETO em Domain.Services: sufixo *Output, namespace *.Outputs
namespace ShopDemo.Auth.Domain.Services.Outputs;
public readonly record struct UserDeactivationOutput(
    int RevokedRefreshTokenCount,
    int RevokedServiceClientCount,
    int RevokedApiKeyCount
);
```

### Por Que Funciona Melhor

1. **Sem ambiguidade**: `Input`/`Output` têm significado único e inequívoco
2. **Sem conflito**: Não colide com `Result<T>` de railway programming
3. **Grep eficiente**: Buscar por `OutputModel` retorna apenas DTOs de saída
4. **Consistência**: Mesma convenção em Messages e Domain.Services

## Consequências

### Benefícios

- **Clareza**: Qualquer desenvolvedor entende imediatamente se é entrada ou saída
- **Navegabilidade**: Namespaces `*.Outputs` agrupam todos os outputs de serviços
- **Previsibilidade**: Dado um serviço, o output está em `*.Outputs` com sufixo `*Output`
- **Automação**: Regras arquiteturais podem validar nomenclatura automaticamente

### Trade-offs (Com Perspectiva)

- **Renomeação**: Código existente com `*Result` precisa ser renomeado
  - É um custo único, compensado pela clareza permanente
- **Convenção rígida**: Todos devem seguir a mesma nomenclatura
  - Regras automatizadas (MS-012a, MS-012b) garantem compliance sem esforço manual

## Fundamentação Teórica

### Padrões de Design Relacionados

**Ubiquitous Language (DDD)**: Termos do codebase devem ser precisos e sem ambiguidade. `Input`/`Output` são termos técnicos com significado bem definido, enquanto `Result` é polissêmico.

**Command-Query Separation (CQS)**: Commands recebem Input e produzem Output (side effects). Queries retornam Results. Manter essa distinção na nomenclatura respeita o princípio CQS.

> "A limited set of well-defined conventions reduces cognitive load and eliminates unnecessary decision-making."
>
> *Um conjunto limitado de convenções bem definidas reduz carga cognitiva e elimina decisões desnecessárias.*

### O Que o Clean Architecture Diz

Clean Architecture usa "Output" para o que sai de um Use Case (Output Port, Output Boundary). Alinhar a nomenclatura com Clean Architecture reforça a consistência conceitual.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Qual a diferença entre Output e Result em Clean Architecture?"
- "Como Ubiquitous Language se aplica a nomenclatura de DTOs?"
- "Por que Result<T> e *ResultModel são conceitos diferentes?"

### Leitura Recomendada

- [Clean Architecture - Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Ubiquitous Language - Martin Fowler](https://martinfowler.com/bliki/UbiquitousLanguage.html)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Messages | Models usam sufixo `*InputModel` / `*OutputModel` |

## Referências no Código

- [DeactivationOutputModel.cs](../../../src/ShopDemo/Auth/Infra.CrossCutting.Messages/V1/Models/DeactivationOutputModel.cs) - output model com sufixo correto
- [UserDeactivationOutput.cs](../../../src/ShopDemo/Auth/Domain/Services/Outputs/UserDeactivationOutput.cs) - domain service output com sufixo correto
- [RegisterUserInputModel.cs](../../../src/ShopDemo/Auth/Infra.CrossCutting.Messages/V1/Models/RegisterUserInputModel.cs) - input model com sufixo correto (referência existente via MS-010)
