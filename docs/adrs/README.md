# Architecture Decision Records (ADRs)

Este diretório contém as decisões arquiteturais do Bedrock, organizadas por categoria.

## Objetivo

Estas ADRs servem como guia para **code agents** (Claude Code, GitHub Copilot, OpenAI Codex) gerarem código consistente com a arquitetura do projeto.

## Categorias de ADRs

| Prefixo | Categoria | Descrição | Status |
|---------|-----------|-----------|--------|
| **CS** | [Code Style](./code-style/README.md) | Organização de código, convenções de namespace e estrutura de diretórios | 3 ADRs |
| **DE** | [Domain Entities](./domain-entities/README.md) | Entidades de domínio, agregados e value objects | 60 ADRs |
| **RE** | Repositories | Persistência e acesso a dados | Em breve |
| **AS** | Application Services | Serviços de aplicação e casos de uso | Em breve |
| **IN** | [Infrastructure](./infrastructure/README.md) | Infraestrutura, cross-cutting concerns | 5 ADRs |
| **RL** | [Relational](./relational/README.md) | Mapeamento objeto-relacional, SQL generation, data models | 4 ADRs |
| **PG** | [PostgreSQL](./postgresql/README.md) | Padroes especificos do PostgreSQL e Npgsql | 2 ADRs |
| **AP** | API | APIs REST, GraphQL, contratos | Em breve |

## Convenção de Nomenclatura

Cada ADR segue o padrão:

```
{PREFIXO}-{NÚMERO}-{slug-descritivo}.md
```

Exemplos:
- `CS-001-interfaces-em-namespace-interfaces.md` - Code Style
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

### Code Style (CS)
- [CS-001: Interfaces em Namespace Interfaces](./code-style/CS-001-interfaces-em-namespace-interfaces.md)
- [CS-002: Lambdas Inline Devem Ser Static em Metodos do Projeto](./code-style/CS-002-lambdas-inline-devem-ser-static.md)
- [CS-003: Logging Sempre com Distributed Tracing](./code-style/CS-003-logging-sempre-com-distributed-tracing.md)
- [Ver todas as ADRs de Code Style](./code-style/)

### Domain Entities (DE)
- [DE-001: Entidades Devem Ser Sealed](./domain-entities/DE-001-entidades-devem-ser-sealed.md)
- [DE-002: Construtores Privados com Factory Methods](./domain-entities/DE-002-construtores-privados-com-factory-methods.md)
- [DE-003: Imutabilidade Controlada (Clone-Modify-Return)](./domain-entities/DE-003-imutabilidade-controlada-clone-modify-return.md)
- [Ver todas as ADRs de Domain Entities](./domain-entities/)

### Infrastructure (IN)
- [IN-001: Camadas Canonicas de um Bounded Context](./infrastructure/IN-001-camadas-canonicas-bounded-context.md)
- [IN-002: Entidades de Dominio Vivem em Projeto Separado](./infrastructure/IN-002-domain-entities-projeto-separado.md)
- [IN-003: Domain É um Projeto Separado de Domain.Entities](./infrastructure/IN-003-domain-projeto-separado.md)
- [IN-004: Modelo de Dados É Detalhe de Implementacao](./infrastructure/IN-004-modelo-dados-detalhe-implementacao.md)
- [IN-005: Infra.Data Atua como Facade de Persistencia](./infrastructure/IN-005-infra-data-facade-persistencia.md)
- [Ver todas as ADRs de Infrastructure](./infrastructure/)

### Relational (RL)
- [RL-001: Mapper Deve Herdar DataModelMapperBase](./relational/RL-001-mapper-herda-datamodelmapperbase.md)
- [RL-002: ConfigureInternal do Mapper Deve Chamar MapTable](./relational/RL-002-mapper-configurar-maptable.md)
- [RL-003: Proibir SQL Literal Fora de Mappers](./relational/RL-003-proibir-sql-fora-de-mapper.md)
- [RL-004: DataModel Deve Ter Apenas Propriedades Primitivas](./relational/RL-004-datamodel-propriedades-primitivas.md)
- [Ver todas as ADRs de Relational](./relational/)

### PostgreSQL (PG)
- [PG-001: MapBinaryImporter Deve Escrever Todas as Colunas](./postgresql/PG-001-binary-importer-todas-colunas.md)
- [PG-002: ConfigureInternal da Connection Deve Validar Connection String](./postgresql/PG-002-connection-validar-connectionstring.md)
- [Ver todas as ADRs de PostgreSQL](./postgresql/)

---

## Fonte

Estas ADRs foram derivadas dos comentários `LLM_GUIDANCE`, `LLM_RULE`, `LLM_TEMPLATE` e `LLM_ANTIPATTERN` documentados no código-fonte.

## Navegação

- [Voltar para docs/](../)
- [AGENTS.md](../../AGENTS.md) - Hub para code agents
