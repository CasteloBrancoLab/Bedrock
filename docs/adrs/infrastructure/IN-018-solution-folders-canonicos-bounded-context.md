# IN-018: Solution Folders Canonicos Para Bounded Context

## Status

Aceita

## Validacao Automatizada

| Rule | Classe | Severidade |
|------|--------|------------|
| IN018 | `IN018_CanonicalSolutionFoldersRule` | Error |

A rule valida que cada projeto de um bounded context esta posicionado
no solution folder correto dentro do arquivo `.sln`, seguindo a
estrutura numerada canonica:

1. **Classificacao da camada**: Usa `ClassifyLayer()` de IN-001 para
   determinar a camada do projeto (Api, Application, Domain, etc.).
2. **Resolucao do solution folder**: Faz parse do `.sln` para extrair
   a cadeia de solution folders (NestedProjects) do projeto.
3. **Validacao do folder**: Compara o solution folder pai contra o
   padrao esperado para a camada (ex: `^\d+ - Api$` para Api).

## Contexto

### O Problema (Analogia)

Imagine um edificio comercial com andares numerados: 1o andar e
recepcao, 2o andar e escritorios, 3o andar e arquivo, 4o andar e
infraestrutura (com sub-andares 4.1 e 4.2). Cada empresa que aluga
um andar sabe exatamente onde esta. Agora imagine que alguem coloca
uma sala de arquivo no andar da recepcao, ou um escritorio no andar
de infraestrutura — visitantes se perdem, entregas vao para o lugar
errado, e a administracao do edificio nao consegue manter a ordem.

### O Problema Tecnico

Em solucoes .NET com multiplos bounded contexts, o arquivo `.sln`
organiza projetos em solution folders. Sem uma convencao clara:

1. **Projetos perdidos**: Um projeto `Infra.Data.PostgreSql` pode
   estar na raiz do solution folder do BC, ou dentro de "Domain",
   ou em qualquer outro lugar.
2. **Inconsistencia entre BCs**: Um BC pode usar folders numerados,
   outro pode usar nomes flat, outro pode nao ter folders.
3. **Navegacao degradada**: O Visual Studio e o Rider usam solution
   folders para agrupar projetos. Sem padrao, a IDE vira uma lista
   plana e longa de projetos.

## Como Normalmente E Feito

### Abordagem 1: Projetos Flat na Raiz

Todos os projetos ficam na raiz do `.sln`, sem solution folders:

```
Solution
├── ShopDemo.Auth.Api.csproj
├── ShopDemo.Auth.Application.csproj
├── ShopDemo.Auth.Domain.csproj
├── ShopDemo.Auth.Domain.Entities.csproj
├── ShopDemo.Auth.Infra.Data.csproj
├── ShopDemo.Auth.Infra.Data.PostgreSql.csproj
├── ShopDemo.Auth.Infra.CrossCutting.Configuration.csproj
├── ShopDemo.Catalog.Api.csproj
├── ShopDemo.Catalog.Application.csproj
└── ... (50 projetos na lista plana)
```

### Abordagem 2: Folders Sem Convencao

Solution folders existem mas sem padrao:

```
Solution
├── Auth
│   ├── Api
│   │   └── ShopDemo.Auth.Api.csproj
│   ├── Infra
│   │   ├── ShopDemo.Auth.Infra.Data.csproj
│   │   └── ShopDemo.Auth.Infra.Data.PostgreSql.csproj
│   └── ShopDemo.Auth.Domain.csproj         ← perdido na raiz do BC
├── Catalog
│   └── ShopDemo.Catalog.Api.csproj         ← sem sub-folders
```

### Por Que Nao Funciona Bem

- **Abordagem 1**: Com mais de 10 projetos, a lista plana torna-se
  impossivel de navegar. Nao ha agrupamento visual por BC ou camada.
- **Abordagem 2**: Cada dev organiza de um jeito. Sem padrao, nao e
  possivel automatizar a validacao nem gerar documentacao consistente.
- **Ambas**: O code agent nao consegue validar programaticamente se
  um projeto esta no lugar certo.

## A Decisao

### Nossa Abordagem

Cada bounded context usa solution folders numerados que refletem as
camadas canonicas definidas em IN-001:

```
src > ShopDemo > Auth >
  1 - Api              → ShopDemo.Auth.Api
  2 - Application      → ShopDemo.Auth.Application
  3 - Domain           → ShopDemo.Auth.Domain.Entities
                         ShopDemo.Auth.Domain
  4 - Infra
    4.1 - Data         → ShopDemo.Auth.Infra.Data
                         ShopDemo.Auth.Infra.Data.PostgreSql
    4.2 - CrossCutting → ShopDemo.Auth.Infra.CrossCutting.Configuration
```

**Mapeamento camada → solution folder:**

| Camada (IN-001) | Solution Folder (regex) | Folder pai |
|-----------------|------------------------|------------|
| Api | `^\d+ - Api$` | BC folder |
| Application | `^\d+ - Application$` | BC folder |
| DomainEntities | `^\d+ - Domain$` | BC folder |
| Domain | `^\d+ - Domain$` | BC folder |
| InfraData | `^\d+\.\d+ - Data$` | `^\d+ - Infra$` |
| InfraDataTech | `^\d+\.\d+ - Data$` | `^\d+ - Infra$` |
| Configuration | `^\d+\.\d+ - CrossCutting$` | `^\d+ - Infra$` |
| Bootstrapper | `^\d+\.\d+ - CrossCutting$` | `^\d+ - Infra$` |

**Regras fundamentais:**

1. **Numeracao reflete a ordem das camadas**: O numero no nome do
   folder corresponde a ordem logica da camada (1=Api, 2=Application,
   3=Domain, 4=Infra).
2. **Sub-numeracao para camadas aninhadas**: Camadas dentro de Infra
   usam sub-numeros (4.1=Data, 4.2=CrossCutting).
3. **BC folder como ancestral**: Todo projeto de BC deve ter o nome
   do BC (ex: "Auth") como ancestral na cadeia de solution folders.
4. **Testes nao validados**: Projetos cujo path comeca com `tests\`
   nao sao validados por esta regra (usam folders flat).
5. **BuildingBlocks nao validados**: Projetos `Bedrock.BuildingBlocks.*`
   nao sao bounded context e seguem sua propria organizacao.

### Por Que Funciona Melhor

- **Previsibilidade**: Qualquer dev ou code agent sabe que o projeto
  Api esta em `<BC> > 1 - Api` sem precisar procurar.
- **Navegacao visual**: A numeracao garante que os folders aparecem
  na ordem logica (Api no topo, Infra embaixo) em qualquer IDE.
- **Validacao automatizada**: A rule IN018 detecta projetos fora do
  lugar antes do merge.
- **Consistencia entre BCs**: Todos os BCs seguem a mesma estrutura.

## Consequencias

### Beneficios

- Navegacao consistente e previsivel na IDE.
- A ordem numerada reflete a arquitetura (de fora para dentro).
- Validacao automatizada previne projetos "perdidos" no `.sln`.
- Novos BCs podem seguir o template sem ambiguidade.

### Trade-offs (Com Perspectiva)

- **Manutencao manual do `.sln`**: Adicionar um novo projeto exige
  posiciona-lo no solution folder correto. Na pratica, o IDE faz
  isso com drag-and-drop, e a rule valida automaticamente.
- **Numeracao rigida**: Se uma nova camada for inserida entre
  camadas existentes, os numeros podem precisar de ajuste. Na
  pratica, as camadas canonicas de IN-001 sao estaveis.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Em qual solution folder devo colocar um projeto Infra.Data.Redis?"
2. "Como o .sln organiza os projetos de um bounded context?"
3. "Qual a relacao entre os numeros dos solution folders e as camadas
   de IN-001?"

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Testing | Fornece a rule `IN018_CanonicalSolutionFoldersRule` que valida a estrutura |

## Referencias no Codigo

- Rule IN018: `src/BuildingBlocks/Testing/Architecture/Rules/InfrastructureRules/IN018_CanonicalSolutionFoldersRule.cs`
- ADR relacionada: [IN-001 — Camadas Canonicas](./IN-001-camadas-canonicas-bounded-context.md)
- Arquivo `.sln` do Bedrock: `Bedrock.sln`
