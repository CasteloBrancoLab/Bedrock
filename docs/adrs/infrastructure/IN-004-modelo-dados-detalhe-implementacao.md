# IN-004: Modelo de Dados É Detalhe de Implementacao

## Status

Aceita

## Contexto

### O Problema (Analogia)

Imagine uma editora que publica o mesmo livro em formatos diferentes:
brochura, e-book e audiobook. O conteudo do livro (a historia, os
personagens) é o mesmo, mas cada formato tem sua propria estrutura.
A brochura tem paginas e capitulos. O e-book tem markup e metadados.
O audiobook tem faixas e duracao. O autor nao precisa saber como cada
formato funciona — ele escreve o livro e a editora adapta para cada
meio. Se o autor tivesse que escrever pensando em todos os formatos
ao mesmo tempo, o livro ficaria poluido com detalhes tecnicos.

### O Problema Tecnico

Entidades de dominio representam conceitos de negocio
([IN-002](./IN-002-domain-entities-projeto-separado.md)). Bancos de
dados representam dados em estruturas especificas de cada tecnologia.
A mesma entidade `User` pode ter representacoes completamente
diferentes dependendo da tecnologia:

- **PostgreSQL**: Tabelas normalizadas com foreign keys, colunas
  tipadas, constraints.
- **MongoDB**: Documento JSON self-contained em uma unica colecao.
- **Redis**: Chave composta (`tenant:user:{id}`) com valor serializado.

Se a entidade de dominio conhecer essas estruturas (tabelas, colunas,
documentos, chaves), o dominio fica acoplado a decisoes de
infraestrutura — violando o principio fundamental do DDD.

## Como Normalmente É Feito

### Abordagem Tradicional

A maioria dos projetos usa ORM (Object-Relational Mapper) com entidades
de dominio mapeadas diretamente para tabelas. O ORM se encarrega do
mapeamento, do change tracking via proxies, das migrations e do
gerenciamento do ciclo de vida dos objetos:

```csharp
// Entidade de dominio com anotacoes de banco
[Table("users")]
public class User
{
    [Key]
    public Guid Id { get; set; }

    [Column("email")]
    [MaxLength(255)]
    public string Email { get; set; }

    [Column("hashed_password")]
    public string HashedPassword { get; set; }
}
```

Na pratica, o acoplamento vai alem das anotacoes. O ORM registra
proxies nas entidades para rastrear mudancas (change tracking),
exige que propriedades sejam `virtual` ou tenham setters publicos,
e gerencia o ciclo de vida dos objetos internamente. A entidade
de dominio deixa de ser um objeto de negocio puro e passa a ser
um artefato hibrido — parte dominio, parte infraestrutura.

### Por Que Nao Funciona Bem

- A entidade de dominio conhece detalhes de persistencia (nomes de
  tabelas, colunas, constraints).
- O ORM acopla o modelo de dominio ao modelo relacional via proxies,
  change tracking e convencoes de mapeamento. A entidade precisa
  atender requisitos do ORM (setters publicos, construtores sem
  parametros, propriedades `virtual`) que conflitam com boas praticas
  de dominio (imutabilidade, construtores privados, factory methods).
- Trocar de banco exige modificar a entidade de dominio.
- Uma mesma entidade nao pode ter representacoes diferentes para
  tecnologias diferentes (ex: PostgreSQL + Redis).
- O modelo de dominio fica poluido com atributos de infraestrutura que
  nao tem significado de negocio.
- Migracoes de banco impactam diretamente o modelo de dominio.

## A Decisao

### Nossa Abordagem

Cada tecnologia de persistencia define seu proprio modelo de dados
(DataModel), completamente independente da entidade de dominio. A
conversao entre os dois é responsabilidade exclusiva da camada
tecnologica.

**Estrutura para a mesma entidade `User`:**

```
samples/ShopDemo/Auth/
  Domain.Entities/
    Users/
      User.cs                              # Entidade de dominio — zero conhecimento de banco

  Infra.Data.PostgreSql/
    DataModels/
      UserDataModel.cs                     # Representacao PostgreSQL
    Factories/
      UserFactory.cs                       # DataModel → Entidade
    Adapters/
      UserDataModelAdapter.cs              # Entidade → DataModel

  Infra.Data.MongoDB/                      # Hipotetico futuro
    DataModels/
      UserDocument.cs                      # Representacao MongoDB
    Factories/
      UserFactory.cs                       # Document → Entidade
    Adapters/
      UserDocumentAdapter.cs               # Entidade → Document
```

**A mesma entidade, representacoes diferentes:**

```csharp
// Domain.Entities — nao sabe nada de banco
public sealed class User : AggregateRootBase<User>
{
    public Email Email { get; }
    public HashedPassword Password { get; }
}

// Infra.Data.PostgreSql — representacao relacional
public sealed class UserDataModel : DataModelBase
{
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string PasswordSalt { get; set; }
}

// Infra.Data.MongoDB — representacao documental (hipotetico)
public sealed class UserDocument
{
    public string Id { get; set; }
    public string Email { get; set; }
    public PasswordSubDocument Password { get; set; }
}
```

**Regras fundamentais:**

1. **O dominio nao conhece DataModel**: Nenhuma referencia a tabelas,
   colunas, schemas, documentos ou chaves.
2. **Cada `Infra.Data.{Tech}` define seus proprios DataModels**: A
   representacao é especifica da tecnologia.
3. **Factories convertem DataModel para entidade** (`DataModel →
   Entity`): Reconstroem a entidade a partir dos dados persistidos.
4. **Adapters convertem entidade para DataModel** (`Entity →
   DataModel`): Preparam os dados para persistencia.
5. **A conversao é responsabilidade exclusiva da camada tecnologica**:
   O dominio nunca participa da conversao.

### Por Que Funciona Melhor

- **Dominio puro**: Entidades expressam apenas conceitos de negocio,
  sem poluicao de infraestrutura.
- **Troca de tecnologia isolada**: Adicionar MongoDB nao altera nenhuma
  entidade — basta criar novos DataModels, Factories e Adapters.
- **Representacoes otimizadas**: Cada tecnologia pode ter a
  representacao mais eficiente para seu paradigma (normalizado para
  SQL, desnormalizado para documentos).
- **Evolucao independente**: Migracoes de banco alteram DataModels, nao
  entidades. Refatoracoes de dominio alteram entidades, nao DataModels
  (as Factories/Adapters absorvem a diferenca).

### Por Que Nao Usamos ORM

Um ORM (como Entity Framework, Hibernate, Dapper com extensions) é uma
tecnologia valida e amplamente adotada. Ele tem seu cenario: quando o
objetivo é tangibilizar a persistencia relacional em um modelo orientado
a objetos e criar uma abstracao simplificada para acesso a dados, o
ORM cumpre bem esse papel.

**O problema é que, na maioria dos projetos, o ORM nao é escolhido por
essa fundamentacao.** É escolhido pela facilidade de uso — scaffolding
rapido, migrations automaticas, LINQ integrado — sem uma analise
consciente do acoplamento que ele introduz. Quando esse acoplamento nao
é intencional e bem fundamentado, ele se torna uma divida tecnica
silenciosa.

**O que o ORM exige da entidade de dominio:**

- Setters publicos ou `protected` (para o change tracker modificar
  propriedades).
- Construtores sem parametros (para o ORM instanciar via reflection).
- Propriedades `virtual` (para geracão de proxies de lazy loading).
- Navegacoes e colecoes tipadas como `ICollection<T>` (para
  relacionamentos).

Esses requisitos conflitam diretamente com boas praticas de dominio:
entidades sealed, construtores privados com factory methods,
imutabilidade controlada, value objects encapsulados. O dominio passa a
ser moldado pelas necessidades do ORM, nao pelas regras de negocio.

**Por que o Bedrock nao adota ORM:**

1. **Multiplos modelos de dados**: O projeto visa atender diferentes
   tecnologias de persistencia (PostgreSQL, MongoDB, Redis, etc.). Nem
   tudo cabe em um modelo relacional, e um ORM é fundamentalmente
   relacional. Em vez de escolher entre ORM e outra abordagem caso a
   caso, adotamos uma abordagem uniforme: DataModels + Factories +
   Adapters para qualquer tecnologia.

2. **Separacao total entre dominio e persistencia**: Sem ORM, a
   entidade de dominio nao precisa atender nenhum requisito de
   infraestrutura. Pode ser `sealed`, ter construtores privados, usar
   factory methods, ser genuinamente imutavel — sem compromissos.

3. **Conversao explicita e testavel**: Factories e Adapters tornam a
   conversao entre dominio e dados explícita, visivel e testavel.
   Com ORM, a conversao é implicita (feita internamente pelo framework)
   e frequentemente surpreende com comportamentos inesperados (lazy
   loading N+1, detached entities, tracking conflicts).

4. **Consistencia arquitetural**: Uma unica abordagem para todas as
   tecnologias é mais previsivel do que alternar entre "aqui usamos
   EF Core" e "ali usamos Dapper" dependendo do caso.

> **Nota**: Nao usar ORM nao significa que o ORM é uma tecnologia ruim.
> Significa que ele nao se encaixa no modelo arquitetural escolhido
> para este projeto, onde o dominio deve ser genuinamente independente
> de qualquer decisao de persistencia.

## Consequencias

### Beneficios

- Entidades de dominio sao genuinamente independentes de tecnologia.
- Suporte a multiplas tecnologias simultaneamente (PostgreSQL para
  OLTP, Redis para cache) sem conflito de representacao.
- Factories e Adapters sao pontos unicos de conversao — faceis de
  testar e manter.
- Code agents geram DataModels na camada correta sem poluir o dominio.

### Trade-offs (Com Perspectiva)

- **Mais classes por entidade**: Cada entidade persistida ganha
  DataModel + Factory + Adapter por tecnologia. Na pratica, essas
  classes sao simples (mapeamento direto) e o custo de manutencao é
  baixo.
- **Conversoes em tempo de execucao**: Cada leitura/escrita passa por
  Factory/Adapter. O custo é negligivel comparado ao tempo de I/O do
  banco de dados (microsegundos de conversao vs. milissegundos de
  query).
- **Duplicacao aparente de propriedades**: `User.Email` e
  `UserDataModel.Email` parecem duplicados, mas representam conceitos
  diferentes — um é regra de negocio, outro é estrutura de
  armazenamento.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Data Mapper** (Fowler, POEAA): Factories e Adapters implementam o
  padrao Data Mapper — uma camada que transfere dados entre objetos e
  banco de dados mantendo ambos independentes.
- **Adapter Pattern** (GoF): Adapters convertem a interface da entidade
  para a interface esperada pela tecnologia de persistencia.
- **Factory Method** (GoF): Factories encapsulam a logica de criacao de
  entidades a partir de dados persistidos.

### O Que o DDD Diz

> "The domain model should be free of technical concerns."
>
> *O modelo de dominio deve ser livre de preocupacoes tecnicas.*

Evans (2003) é enfatico: o modelo de dominio nao deve conhecer a forma
como é persistido. DataModels como artefatos exclusivos da camada
tecnologica respeitam essa separacao.

### O Que o Clean Architecture Diz

> "The database is a detail."
>
> *O banco de dados é um detalhe.*

Robert C. Martin (2017). DataModels materializam essa ideia: o banco é
um detalhe de implementacao que vive na camada mais externa, sem
contaminar o nucleo.

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Qual a diferenca entre DataModel e Entity no Bedrock?"
2. "Por que nao usar atributos de ORM diretamente nas entidades de
   dominio?"
3. "Como Factories e Adapters permitem suporte a multiplos bancos de
   dados?"

### Leitura Recomendada

- Eric Evans, *Domain-Driven Design* (2003), Cap. 6 — Repositories
- Martin Fowler, *Patterns of Enterprise Application Architecture*
  (2002), Cap. 10 — Data Mapper
- Robert C. Martin, *Clean Architecture* (2017), Cap. 30 — The Database
  Is a Detail

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Persistence.PostgreSql | Framework base para DataModels, Factories e Adapters PostgreSQL (DataModelBase, DataModelBaseFactory, DataModelBaseAdapter) |
| Bedrock.BuildingBlocks.Domain.Entities | Framework base para entidades de dominio que os DataModels representam |

## Referencias no Codigo

- DataModel de exemplo: `samples/ShopDemo/Auth/Infra.Data.PostgreSql/DataModels/UserDataModel.cs`
- Factory de exemplo: `samples/ShopDemo/Auth/Infra.Data.PostgreSql/Factories/UserFactory.cs`
- Adapter de exemplo: `samples/ShopDemo/Auth/Infra.Data.PostgreSql/Adapters/UserDataModelAdapter.cs`
- ADR relacionada: [IN-002 — Domain.Entities Projeto Separado](./IN-002-domain-entities-projeto-separado.md)
- ADR relacionada: [IN-003 — Domain Projeto Separado](./IN-003-domain-projeto-separado.md)
