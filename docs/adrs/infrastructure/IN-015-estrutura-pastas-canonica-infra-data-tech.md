# IN-015: Estrutura Canonica de Pastas em Infra.Data.{Tech}

## Status

Aceita

## Validacao Automatizada

Esta ADR sera validada pela rule de arquitetura
**IN015_CanonicalFolderStructureRule**, que verifica:

- Projetos `*.Infra.Data.{Tech}` com pelo menos um DataModel devem
  conter namespaces correspondentes as pastas canonicas.
- A presenca de um DataModel implica a existencia dos namespaces:
  `*.Connections.Interfaces`, `*.UnitOfWork.Interfaces`,
  `*.DataModels`, `*.DataModelsRepositories.Interfaces`,
  `*.Factories`, `*.Adapters`, `*.Repositories.Interfaces`.
- **Nota**: `*.Mappers` e validado pela regra [RL-001](../relational/RL-001-mapper-herda-datamodelmapperbase.md)
  (categoria Relational), nao por esta regra.

## Contexto

### O Problema (Analogia)

Imagine uma rede de franquias de restaurantes. Toda franquia segue o
mesmo layout: cozinha no fundo, balcao de atendimento na frente,
estoque a esquerda, banheiros a direita. Qualquer funcionario
transferido de uma franquia para outra sabe imediatamente onde fica
cada coisa. Se cada franquia inventasse seu proprio layout, cada
transferencia seria como comecar do zero.

### O Problema Tecnico

Projetos `Infra.Data.{Tech}` contem varios tipos de artefatos:
conexoes, unit of work, data models, mappers, repositories, factories
e adapters. Sem uma estrutura de pastas padronizada:

- Code agents criam pastas com nomes diferentes a cada iteracao
  (`Repository` vs. `Repositories`, `Models` vs. `DataModels`).
- Novos desenvolvedores perdem tempo navegando estruturas inconsistentes.
- Rules de arquitetura nao conseguem validar a presenca de artefatos
  obrigatorios.

## Como Normalmente E Feito

### Abordagem Tradicional

Cada projeto inventa sua propria estrutura:

```
# Projeto A — organizado por tipo
Infra.Data.PostgreSql/
  Models/
  Repos/
  Config/

# Projeto B — organizado por entidade
Infra.Data.PostgreSql/
  Users/
    UserModel.cs
    UserRepo.cs
    UserMapper.cs
  Orders/
    OrderModel.cs
    OrderRepo.cs
```

### Por Que Nao Funciona Bem

- **Inconsistencia entre BCs**: Cada BC usa nomenclatura diferente para
  os mesmos conceitos.
- **Code agents desorientados**: Sem convencao, LLMs geram estruturas
  diferentes a cada vez.
- **Validacao impossivel**: Rules de arquitetura nao conseguem verificar
  se um artefato existe quando a pasta pode ter qualquer nome.

## A Decisao

### Nossa Abordagem

Todo projeto `Infra.Data.{Tech}` deve seguir esta estrutura:

```
{BC}.Infra.Data.{Tech}/
├── {BC}.Infra.Data.{Tech}.csproj
├── GlobalUsings.cs
├── Adapters/
│   └── {Entity}DataModelAdapter.cs        (IN-014)
├── Connections/
│   ├── Interfaces/
│   │   └── I{BC}{Tech}Connection.cs       (IN-006)
│   └── {BC}{Tech}Connection.cs            (IN-008)
├── DataModels/
│   └── {Entity}DataModel.cs               (IN-010)
├── DataModelsRepositories/
│   ├── Interfaces/
│   │   └── I{Entity}DataModelRepository.cs (IN-011)
│   └── {Entity}DataModelRepository.cs
├── Factories/
│   ├── {Entity}Factory.cs                 (IN-013)
│   └── {Entity}DataModelFactory.cs        (IN-013)
├── Mappers/
│   └── {Entity}DataModelMapper.cs         (RL-001)
├── Repositories/
│   ├── Interfaces/
│   │   └── I{Entity}{Tech}Repository.cs   (IN-012)
│   └── {Entity}{Tech}Repository.cs
└── UnitOfWork/
    ├── Interfaces/
    │   └── I{BC}{Tech}UnitOfWork.cs       (IN-007)
    └── {BC}{Tech}UnitOfWork.cs            (IN-009)
```

**Relacao com outras ADRs:** Cada pasta corresponde a uma ou mais ADRs
que definem o conteudo obrigatorio (indicadas entre parenteses acima).

**Regras fundamentais:**

1. **Uma Connection e um UnitOfWork por BC**: Compartilhados por todas
   as entidades do BC.
2. **Um DataModel, Mapper, Factory (x2), Adapter e Repository por
   aggregate root**: Cada entidade persistida gera 6 artefatos
   especificos.
3. **Interfaces em subpasta `Interfaces/`**: Consistente com
   [CS-001](../code-style/CS-001-interfaces-em-namespace-interfaces.md).
4. **Nomenclatura consistente**: Todas as classes seguem o padrao
   `{Entity}{Sufixo}` (ex: `UserDataModel`, `UserFactory`,
   `UserDataModelMapper`).

**Exemplo concreto (ShopDemo.Auth):**

```
ShopDemo.Auth.Infra.Data.PostgreSql/
├── Adapters/
│   └── UserDataModelAdapter.cs
├── Connections/
│   ├── Interfaces/
│   │   └── IAuthPostgreSqlConnection.cs
│   └── AuthPostgreSqlConnection.cs
├── DataModels/
│   └── UserDataModel.cs
├── DataModelsRepositories/
│   ├── Interfaces/
│   │   └── IUserDataModelRepository.cs
│   └── UserDataModelRepository.cs
├── Factories/
│   ├── UserFactory.cs
│   └── UserDataModelFactory.cs
├── Mappers/
│   └── UserDataModelMapper.cs
├── Repositories/
│   ├── Interfaces/
│   │   └── IUserPostgreSqlRepository.cs
│   └── UserPostgreSqlRepository.cs
└── UnitOfWork/
    ├── Interfaces/
    │   └── IAuthPostgreSqlUnitOfWork.cs
    └── AuthPostgreSqlUnitOfWork.cs
```

### Por Que Funciona Melhor

- **Previsibilidade total**: Qualquer desenvolvedor ou code agent sabe
  exatamente onde encontrar e criar cada artefato.
- **Validacao automatizada**: Rules de arquitetura podem verificar se
  todos os artefatos obrigatorios existem.
- **Onboarding rapido**: Novos devs navegam a estrutura de qualquer BC
  imediatamente — e identica em todos.
- **Scaffold automatico**: Ferramentas de geracao de codigo podem criar
  a estrutura completa para um novo aggregate root.

## Consequencias

### Beneficios

- Estrutura identica em todos os BCs — zero improvisacao.
- Code agents geram artefatos no lugar certo.
- Rules de arquitetura validam completude.
- Navegacao previsivel em IDEs.

### Trade-offs (Com Perspectiva)

- **Estrutura "rigida"**: Nao ha espaco para organizacao criativa de
  pastas. Isso e intencional — consistencia supera criatividade quando
  o objetivo e previsibilidade.
- **Muitas pastas para poucos arquivos**: Um BC com um unico aggregate
  root tera 8 pastas com 1 arquivo cada. Na pratica, BCs tipicos tem
  3-5 aggregate roots, preenchendo a estrutura naturalmente.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Convention over Configuration** (Ruby on Rails): A estrutura de
  pastas e uma convencao que elimina a necessidade de configuracao
  explicita. O framework sabe onde procurar cada tipo de artefato.
- **Screaming Architecture** (Robert C. Martin): A estrutura de pastas
  "grita" a intencao — Connections, UnitOfWork, DataModels, Repositories
  sao auto-explicativos.

### O Que o DDD Diz

> "Ubiquitous Language applies to code structure too."
>
> *A Linguagem Ubiqua se aplica a estrutura do codigo tambem.*

Evans (2003). Pastas com nomes que refletem conceitos arquiteturais
(Connection, UnitOfWork, Repository) sao parte da linguagem ubiqua
da equipe de infraestrutura.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Quais artefatos sao criados por aggregate root no Bedrock?"
2. "Qual a diferenca entre Factories e Adapters na estrutura de pastas?"
3. "Como adicionar um novo aggregate root a um Infra.Data.{Tech}
   existente?"

### Leitura Recomendada

- Robert C. Martin, *Clean Architecture* (2017), Cap. 21 — Screaming
  Architecture
- Eric Evans, *Domain-Driven Design* (2003), Cap. 5 — Layered
  Architecture

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Persistence.PostgreSql | Define as base classes que as pastas contem (ConnectionBase, UnitOfWorkBase, DataModelBase, etc.) |

## Referencias no Codigo

- Estrutura completa de exemplo: `src/ShopDemo/Auth/Infra.Data.PostgreSql/`
- Template de referencia: `src/Templates/Infra.Data.PostgreSql/`
- ADR relacionada: [IN-001 — Camadas Canonicas](./IN-001-camadas-canonicas-bounded-context.md)
