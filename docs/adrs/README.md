# Architecture Decision Records (ADRs)

Este diretório contém as decisões arquiteturais do Bedrock, organizadas por categoria.

## Objetivo

Estas ADRs servem como guia para **code agents** (Claude Code, GitHub Copilot, OpenAI Codex) gerarem código consistente com a arquitetura do projeto.

## Categorias de ADRs

| Prefixo | Categoria | Descrição | Status |
|---------|-----------|-----------|--------|
| **DE** | [Domain Entities](./domain-entities/README.md) | Entidades de domínio, agregados e value objects | 58 ADRs |
| **RE** | Repositories | Persistência e acesso a dados | Em breve |
| **AS** | Application Services | Serviços de aplicação e casos de uso | Em breve |
| **IN** | Infrastructure | Infraestrutura, cross-cutting concerns | Em breve |
| **AP** | API | APIs REST, GraphQL, contratos | Em breve |

## Convenção de Nomenclatura

Cada ADR segue o padrão:

```
{PREFIXO}-{NÚMERO}-{slug-descritivo}.md
```

Exemplos:
- `DE-001-entidades-devem-ser-sealed.md` - Domain Entities
- `RE-001-repository-pattern.md` - Repositories
- `AS-001-cqrs-segregation.md` - Application Services

## Status das ADRs

- **Proposta**: ADR identificada, aguardando documentação completa
- **Aceita**: ADR documentada e aprovada para implementação
- **Obsoleta**: ADR substituída por outra decisão

## Estrutura de uma ADR

Cada ADR segue o formato abaixo, projetado para ser **educacional** tanto para LLMs quanto para desenvolvedores:

```markdown
# {PREFIXO}-XXX: Título

## Status
Proposta | Aceita | Obsoleta

## Contexto

### O Problema (Analogia)
Uma analogia lúdica e acessível que qualquer pessoa entende.

### O Problema Técnico
Descrição técnica do problema que a decisão resolve.

## Como Normalmente É Feito

### Abordagem Tradicional
Como a maioria dos projetos/desenvolvedores resolve isso.

### Por Que Não Funciona Bem
Os problemas dessa abordagem tradicional.

## A Decisão

### Nossa Abordagem
A decisão arquitetural tomada neste projeto.

### Por Que Funciona Melhor
Explicação clara dos benefícios.

## Consequências

### Benefícios
- Benefício 1
- Benefício 2

### Trade-offs (Com Perspectiva)
- O que abrimos mão ao adotar essa decisão
- IMPORTANTE: Sempre contextualize custos com comparações práticas
  - Evite afirmações soltas como "cria alocações" sem comparar com operações do dia-a-dia
  - Use exemplos concretos (LINQ, List.Add, chamadas HTTP) como base de comparação
  - Indique quando o custo seria REALMENTE relevante vs. quando é negligenciável

## Fundamentação Teórica

Esta seção conecta a decisão com literatura e padrões reconhecidos.
Inclua APENAS o que for pertinente - nem toda ADR terá relação com todos os tópicos.

### Padrões de Design Relacionados
Quais design patterns (GoF, POEAA, etc.) fundamentam esta decisão?
Cite o padrão, explique brevemente como se aplica.

### O Que o DDD Diz
Domain-Driven Design (Evans, Vernon) - como esta decisão se alinha com tactical/strategic patterns?

### O Que o Clean Code Diz
Princípios do Clean Code (Robert C. Martin) - quais princípios suportam esta decisão?

### O Que o Clean Architecture Diz
Clean Architecture (Robert C. Martin) - como esta decisão respeita boundaries e dependency rules?

### Outros Fundamentos
SOLID, GRASP, Effective Java, ou outras referências relevantes.

**IMPORTANTE**: O objetivo NÃO é fazer "carteirada" citando buzzwords.
É mostrar a fundamentação real, incluindo quando a literatura NÃO se aplica ou quando
estamos fazendo uma escolha diferente do convencional (e por quê).

**TRADUÇÃO**: Sempre que citar texto originalmente em inglês, inclua a tradução livre
em português logo abaixo, em itálico. Exemplo:

```markdown
> "Design and document for inheritance or else prohibit it."
>
> *Projete e documente para herança ou então proíba-a.*
```

## Aprenda Mais

### Perguntas Para Fazer à LLM
Sugestões de perguntas para aprofundar o entendimento.

### Leitura Recomendada
Links para artigos, livros, talks relevantes.

## Building Blocks Correlacionados

Liste os Building Blocks do framework que implementam ou se relacionam com esta decisão:

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [Nome](../building-blocks/path/to/doc.md) | Breve explicação de como o building block implementa ou se relaciona com a decisão |

**Nota:** Consulte o [Índice de Building Blocks](../building-blocks/README.md) para a lista completa.

## Referências no Código
Links para implementações de referência no repositório.
```

## Navegação Rápida

### Domain Entities (DE)
- [DE-001: Entidades Devem Ser Sealed](./domain-entities/DE-001-entidades-devem-ser-sealed.md)
- [DE-002: Construtores Privados com Factory Methods](./domain-entities/DE-002-construtores-privados-com-factory-methods.md)
- [DE-003: Imutabilidade Controlada (Clone-Modify-Return)](./domain-entities/DE-003-imutabilidade-controlada-clone-modify-return.md)
- [Ver todas as ADRs de Domain Entities](./domain-entities/)

---

## Fonte

Estas ADRs foram derivadas dos comentários `LLM_GUIDANCE`, `LLM_RULE`, `LLM_TEMPLATE` e `LLM_ANTIPATTERN` documentados no código-fonte.

## Navegação

- [Voltar para docs/](../)
- [AGENTS.md](../../AGENTS.md) - Hub para code agents
