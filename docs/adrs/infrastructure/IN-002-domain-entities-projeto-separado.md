# IN-002: Entidades de Dominio Vivem em Projeto Separado

## Status

Aceita

## Contexto

### O Problema (Analogia)

Imagine uma empresa que tem um catalogo de produtos. Esse catalogo é
usado por todos os departamentos — vendas consulta os produtos, logistica
precisa dos pesos e dimensoes, financeiro precisa dos precos. Se o
catalogo ficar trancado dentro da sala do departamento de vendas, todos
os outros precisam passar por vendas para consultar qualquer informacao.
Agora imagine que o catalogo fica em uma biblioteca central, acessivel
a todos, sem depender de nenhum departamento especifico. Qualquer area
consulta diretamente.

### O Problema Tecnico

Em projetos DDD, entidades de dominio (entities, value objects,
aggregates) sao o vocabulario compartilhado de todo o bounded context.
Todas as camadas precisam referenciar essas entidades:

1. **Domain** precisa das entidades para definir interfaces de
   repositorio e domain services.
2. **Application** precisa das entidades para orquestrar casos de uso.
3. **Infra.Data** precisa das entidades para converter entre modelo de
   dominio e modelo de dados.
4. **Infra.Data.{Tech}** precisa das entidades para criar factories e
   adapters.

Se as entidades vivem dentro do projeto Domain junto com interfaces de
repositorio e domain services, todas as camadas de infraestrutura
precisam referenciar Domain — o que cria dependencias desnecessarias e
viola a separacao de responsabilidades.

## Como Normalmente É Feito

### Abordagem Tradicional

A maioria dos projetos coloca tudo no mesmo projeto `Domain`:

```
MyBoundedContext/
  Domain/
    Entities/
      User.cs
      Order.cs
    ValueObjects/
      Email.cs
    Interfaces/
      IUserRepository.cs
    Services/
      AuthenticationService.cs
```

### Por Que Nao Funciona Bem

- Camadas de infraestrutura que precisam apenas das entidades acabam
  referenciando o projeto Domain inteiro, ganhando visibilidade de
  interfaces e services que nao deveriam conhecer.
- `Infra.Data.PostgreSql` precisa apenas de `User` para criar um
  `UserDataModel`, mas acaba referenciando `IUserRepository` e
  `AuthenticationService` — artefatos que nao lhe dizem respeito.
- A dependencia excessiva dificulta a compilacao incremental: qualquer
  mudanca em um domain service recompila todas as camadas de infra.
- Code agents nao tem uma regra clara sobre o que vai em qual projeto e
  misturam responsabilidades.

## A Decisao

### Nossa Abordagem

Entidades de dominio vivem em um projeto separado chamado
`{BC}.Domain.Entities`. Este projeto contem exclusivamente:

- **Entidades** (aggregate roots e entidades filhas)
- **Value objects**
- **Enumeracoes de dominio**
- **Interfaces de entidade** (ex: `IUser`, `IAggregateRoot`)

```
samples/ShopDemo/Auth/
  Domain.Entities/
    ShopDemo.Auth.Domain.Entities.csproj
    Users/
      User.cs
      Interfaces/
        IUser.cs
    ValueObjects/
      Email.cs
      HashedPassword.cs
```

**Regras fundamentais:**

1. **Zero dependencias externas** alem do framework Bedrock
   (`Bedrock.BuildingBlocks.Domain.Entities`).
2. **Nenhuma referencia a infraestrutura**: sem DbContext, sem
   connection strings, sem serializers.
3. **Compartilhavel por todas as camadas**: Domain, Application,
   Infra.Data e Infra.Data.{Tech} referenciam Domain.Entities.
4. **Separado de Domain**: Domain.Entities nao contem interfaces de
   repositorio nem domain services — esses vivem em `{BC}.Domain`.

### Por Que Funciona Melhor

- **Minimo de dependencias**: Camadas de infra referenciam apenas
  Domain.Entities, nao o Domain inteiro.
- **Compilacao incremental eficiente**: Mudancas em domain services ou
  interfaces de repositorio nao recompilam camadas de infra.
- **Clareza de responsabilidade**: Entidades sao o nucleo do BC,
  acessiveis a todos, sem arrastar dependencias desnecessarias.
- **Previsibilidade para code agents**: Regra simples — entidade vai
  em Domain.Entities, interface de repositorio vai em Domain.

### Cenarios Reais de Reuso

A separacao em projeto dedicado com zero dependencias de infraestrutura
abre possibilidades que um Domain monolitico nao permite:

1. **Simuladores e testes em memoria**: Um simulador de carga ou um
   test harness pode referenciar `Auth.Domain.Entities` para criar
   `User`, `Email` e `HashedPassword` em memoria — sem arrastar
   DbContext, connection strings ou qualquer dependencia de banco.
   Testes de integracao constroem cenarios inteiros usando entidades
   reais sem precisar de infraestrutura.

2. **Plugins e integrações externas**: Um plugin de Excel para
   importacao de usuarios pode referenciar `Auth.Domain.Entities` para
   validar `Email` e `HashedPassword` localmente, antes de enviar para
   a API. O plugin roda no desktop do usuario, sem banco de dados, e
   ainda assim usa as mesmas regras de validacao do dominio. O mesmo
   vale para CLIs, scripts de migracao ou ferramentas de auditoria.

3. **Consumo intencional entre bounded contexts**: Em cenarios onde a
   latencia de rede ou o custo de I/O tornam chamadas entre BCs
   inaceitaveis, um BC pode referenciar diretamente o
   `Domain.Entities` de outro BC. Por exemplo, o BC de Billing pode
   referenciar `Auth.Domain.Entities` para validar regras de `User`
   localmente em vez de fazer uma chamada HTTP ao Auth. Isso é uma
   decisao consciente de trade-off (acoplamento vs. performance) que
   so é viavel porque Domain.Entities nao arrasta dependencias de
   infraestrutura.

4. **Geracao de documentacao e schemas**: Ferramentas de geracao de
   documentacao, schemas JSON ou contratos de API podem referenciar
   Domain.Entities para introspectar entidades e value objects via
   reflection — sem instanciar banco, sem configurar DI, sem
   dependencias transitivas.

> **Nota**: O cenario 3 (consumo entre BCs) é uma excecao consciente ao
> principio de isolamento de bounded contexts. Deve ser documentado como
> decisao arquitetural e justificado por requisitos de performance
> mensurados — nunca por conveniencia.

## Consequencias

### Beneficios

- Todas as camadas do BC podem referenciar entidades sem depender de
  domain services ou interfaces de repositorio.
- O projeto Domain.Entities se torna o vocabulario universal do BC.
- Menor acoplamento entre camadas: `Infra.Data.PostgreSql` depende de
  `Domain.Entities`, nao de `Domain`.
- Code agents sabem exatamente onde criar entidades e value objects.

### Trade-offs (Com Perspectiva)

- **Mais um projeto por BC**: Um projeto adicional na solution. Na
  pratica, o custo de build é negligivel (milissegundos) e o ganho em
  organizacao compensa amplamente.
- **Dois projetos com prefixo Domain**: `Domain.Entities` e `Domain`
  podem confundir iniciantes. A convencao de nomes e a documentacao
  (esta ADR) eliminam a ambiguidade.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Shared Kernel** (DDD): Domain.Entities funciona como um shared
  kernel dentro do bounded context — o vocabulario que todas as camadas
  compartilham.

### O Que o DDD Diz

> "The model is the backbone of the language used by all team members."
>
> *O modelo é a espinha dorsal da linguagem usada por todos os membros
> da equipe.*

Evans (2003) enfatiza que o modelo de dominio é o ponto central de
comunicacao. Isolar entidades em um projeto dedicado materializa essa
centralidade: todas as camadas "falam a mesma lingua" referenciando
o mesmo projeto.

### O Que o Clean Architecture Diz

> "Entities encapsulate enterprise-wide business rules."
>
> *Entidades encapsulam regras de negocio corporativas.*

Robert C. Martin (2017) posiciona entidades no circulo mais interno
da arquitetura, sem dependencias externas. Um projeto separado
`Domain.Entities` com zero dependencias de infraestrutura respeita
essa regra literalmente.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Qual a diferenca entre Domain.Entities e Domain no Bedrock?"
2. "Por que Infra.Data.PostgreSql referencia Domain.Entities e nao
   Domain?"
3. "O que acontece se eu colocar uma interface de repositorio em
   Domain.Entities?"

### Leitura Recomendada

- Eric Evans, *Domain-Driven Design* (2003), Cap. 5 — Entities
- Robert C. Martin, *Clean Architecture* (2017), Cap. 20 — Business
  Rules
- Vaughn Vernon, *Implementing Domain-Driven Design* (2013), Cap. 5 —
  Entities

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Domain.Entities | Framework base que Domain.Entities de cada BC referencia (EntityBase, AggregateRootBase, ValueObject) |

## Referencias no Codigo

- Implementacao de exemplo: `samples/ShopDemo/Auth/Domain.Entities/`
- Entidade de referencia: `samples/ShopDemo/Auth/Domain.Entities/Users/User.cs`
- ADR relacionada: [IN-001 — Camadas Canonicas de um Bounded Context](./IN-001-camadas-canonicas-bounded-context.md)
