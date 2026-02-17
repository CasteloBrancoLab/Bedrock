# IN-017: Cada Camada Tem Seu Proprio Bootstrapper Para IoC

## Status

Aceita

## Validacao Automatizada

| Rule | Classe | Severidade |
|------|--------|------------|
| IN017 | `IN017_BootstrapperPerLayerRule` | Error |

A rule realiza duas validacoes nos projetos `Infra.Data.{Tech}` e
`Infra.CrossCutting.Configuration`:

1. **Existencia do Bootstrapper**: O projeto deve possuir uma classe
   `public static class Bootstrapper` no namespace raiz, com metodo
   `ConfigureServices(IServiceCollection)`.
2. **Exclusividade do IoC**: Nenhuma outra classe no projeto pode ter
   metodos que recebam `IServiceCollection` como parametro. Apenas o
   `Bootstrapper` pode registrar servicos no container de IoC.

## Contexto

### O Problema (Analogia)

Imagine um predio de escritorios. Cada andar tem suas proprias salas,
moveis e equipamentos. Se alguem do 10o andar tiver que ligar para a
administracao central para pedir cada cadeira, mesa e computador — e a
administracao tiver que manter uma lista de tudo que cada andar precisa
— qualquer mudanca em um andar exige atualizar a lista central. Agora
imagine que cada andar tem seu proprio responsavel de compras: ele sabe
o que o andar precisa, faz o pedido e organiza. A administracao central
so precisa dizer "andar 10, faca seu setup" — sem conhecer os detalhes.

### O Problema Tecnico

Em projetos com multiplas camadas (Domain, Application, Infra.Data,
Infra.Data.PostgreSql, Infra.CrossCutting.Configuration), o registro
de dependencias no container de IoC precisa de uma estrategia clara:

1. **Composition root monolitico**: Um unico arquivo `Program.cs` ou
   `Startup.cs` registra todas as dependencias de todas as camadas.
   Qualquer nova classe em qualquer camada exige alterar o arquivo
   central.
2. **Conhecimento cruzado**: Se `Infra.Data.PostgreSql` registra os
   servicos de `Infra.CrossCutting.Configuration`, passa a ser
   responsavel por algo que nao e seu.
3. **Duplicacao de registros**: Sem convencao, multiplas camadas podem
   tentar registrar o mesmo servico, ou esquecer de registrar um
   servico que so elas conhecem.

## Como Normalmente E Feito

### Abordagem 1: Extension Methods em ServiceCollectionExtensions

A abordagem mais comum no ecossistema .NET cria classes
`ServiceCollectionExtensions` em namespaces `Registration`:

```csharp
// Namespace: MyApp.Auth.Infra.Data.PostgreSql.Registration
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthPostgreSql(
        this IServiceCollection services)
    {
        // Registra tudo: seus servicos, servicos de outras camadas,
        // e ate servicos de BuildingBlocks
        services.AddBedrockConfiguration<AuthConfigurationManager>();
        services.AddScoped<IAuthConnection, AuthConnection>();
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}
```

### Abordagem 2: Registro Centralizado no Program.cs

Outra abordagem registra tudo no ponto de entrada:

```csharp
// Program.cs — sabe de tudo
services.AddSingleton<AuthConfigurationManager>();
services.AddScoped<IAuthConnection, AuthConnection>();
services.AddScoped<IAuthUnitOfWork, AuthUnitOfWork>();
services.AddSingleton<IDataModelMapper<UserDataModel>, UserDataModelMapper>();
services.AddScoped<IUserDataModelRepository, UserDataModelRepository>();
services.AddScoped<IUserPostgreSqlRepository, UserPostgreSqlRepository>();
services.AddScoped<IUserRepository, UserRepository>();
// ... 50 linhas depois ...
```

### Por Que Nao Funciona Bem

- **Abordagem 1**: A classe `ServiceCollectionExtensions` de uma camada
  registra servicos de outra camada. Isso cria dependencia de
  conhecimento: `Infra.Data.PostgreSql` precisa saber como registrar
  `AuthConfigurationManager`. Se a camada de Configuration mudar sua
  forma de registro, todas as camadas que a registram precisam mudar.
- **Abordagem 2**: O `Program.cs` conhece todas as classes internas de
  todas as camadas. Adicionar uma nova classe em qualquer camada exige
  alterar o ponto de entrada. O arquivo cresce indefinidamente.
- **Ambas**: O nome `ServiceCollectionExtensions` e generico — nao
  expressa que e o ponto de registro da camada. Multiplas classes com o
  mesmo nome em namespaces diferentes confundem a navegacao.
- **Ambas**: Nao ha convencao sobre quem registra o que. O mesmo
  servico pode ser registrado por duas camadas diferentes, ou por
  nenhuma.

## A Decisao

### Nossa Abordagem

Cada projeto que tem servicos para registrar no IoC deve ter um arquivo
`Bootstrapper.cs` na raiz do projeto, com uma classe estatica
`Bootstrapper` e um metodo `ConfigureServices`:

```csharp
namespace ShopDemo.Auth.Infra.Data.PostgreSql;

public static class Bootstrapper
{
    public static IServiceCollection ConfigureServices(
        IServiceCollection services)
    {
        // Registra APENAS os servicos desta camada
        services.TryAddSingleton<IDataModelMapper<UserDataModel>, UserDataModelMapper>();
        services.TryAddScoped<IAuthPostgreSqlConnection, AuthPostgreSqlConnection>();
        services.TryAddScoped<IAuthPostgreSqlUnitOfWork, AuthPostgreSqlUnitOfWork>();
        services.TryAddScoped<IUserDataModelRepository, UserDataModelRepository>();
        services.TryAddScoped<IUserPostgreSqlRepository, UserPostgreSqlRepository>();

        return services;
    }
}
```

**Regras fundamentais:**

1. **Um Bootstrapper por projeto**: Cada projeto `.csproj` que tem
   servicos para registrar possui exatamente um `Bootstrapper.cs` na
   raiz.
2. **Registra apenas seus proprios servicos**: O Bootstrapper de
   `Infra.Data.PostgreSql` registra connections, unit of work, mappers
   e repositories. Nao registra o `AuthConfigurationManager` — isso e
   responsabilidade do Bootstrapper de `Infra.CrossCutting.Configuration`.
3. **Usa `TryAdd*` para idempotencia**: `TryAddSingleton`,
   `TryAddScoped`, `TryAddTransient` evitam duplicacao se o mesmo
   Bootstrapper for chamado mais de uma vez.
4. **Nome e assinatura canonicos**: Sempre `public static class Bootstrapper`
   com metodo `public static IServiceCollection ConfigureServices(IServiceCollection services)`.
5. **Classe `Bootstrapper` no namespace raiz do projeto**: Nao em
   subpasta `Registration/` nem em namespace separado.

**BuildingBlocks seguem o mesmo padrao:**

```csharp
namespace Bedrock.BuildingBlocks.Configuration;

public static class Bootstrapper
{
    public static IServiceCollection AddBedrockConfiguration<TManager>(
        this IServiceCollection services,
        Action<ConfigurationOptions>? configure = null)
        where TManager : ConfigurationManagerBase
    {
        services.TryAddSingleton<TManager>(sp => { ... });
        return services;
    }
}
```

BuildingBlocks podem usar extension methods com nomes descritivos
(`AddBedrockConfiguration`) porque sao bibliotecas reutilizaveis
consumidas por multiplos projetos. O ponto importante e que a classe
se chama `Bootstrapper` e vive no namespace raiz do BuildingBlock.

**Composicao no Composition Root:**

```csharp
// Program.cs ou Infra.CrossCutting.Bootstrapper — conhece apenas Bootstrappers
Auth.Infra.CrossCutting.Configuration.Bootstrapper.ConfigureServices(services);
Auth.Infra.Data.PostgreSql.Bootstrapper.ConfigureServices(services);
Auth.Infra.Data.Bootstrapper.ConfigureServices(services);
```

O Composition Root nao conhece classes internas — so chama
Bootstrappers. Adicionar uma nova classe interna a uma camada nao
exige alterar o Composition Root.

**Lifetimes recomendados por tipo de servico:**

| Tipo de Servico | Lifetime | Motivo |
|-----------------|----------|--------|
| Mappers (`IDataModelMapper<T>`) | Singleton | Stateless, caches internos estaticos |
| ConfigurationManager | Singleton | Configuracao imutavel apos startup |
| Connection (`IPostgreSqlConnection`) | Scoped | Mantém `NpgsqlConnection` por request |
| UnitOfWork (`IPostgreSqlUnitOfWork`) | Scoped | Mantém transacao por request |
| DataModel Repositories | Scoped | Dependem do UoW |
| Repositories tecnologicos | Scoped | Dependem do DataModel Repository |
| Repositories de negocio (Facade) | Scoped | Dependem dos repositories tecnologicos |

### Por Que Funciona Melhor

- **Principio da Responsabilidade Unica**: Cada camada sabe quais
  servicos ela fornece e como registra-los.
- **Zero conhecimento cruzado**: `Infra.Data.PostgreSql` nao sabe como
  `Infra.CrossCutting.Configuration` registra seus servicos.
- **Composicao declarativa**: O Composition Root e uma lista de
  chamadas a Bootstrappers — facil de ler, facil de ordenar.
- **Idempotente**: `TryAdd*` garante que chamar o mesmo Bootstrapper
  duas vezes nao causa erro.

## Consequencias

### Beneficios

- Cada camada e autossuficiente para registro de IoC.
- Adicionar uma nova classe interna a uma camada nao exige alterar
  nenhum outro projeto.
- Composicao no `Program.cs` e declarativa e concisa.
- Convencao canonica: qualquer dev ou code agent sabe que o registro
  de IoC esta em `Bootstrapper.cs` na raiz do projeto.
- `TryAdd*` previne conflitos de registro duplicado.

### Trade-offs (Com Perspectiva)

- **Bootstrapper por projeto**: Projetos com poucos servicos (ex:
  `Infra.CrossCutting.Configuration` com 1 registro) ainda precisam
  de um `Bootstrapper.cs`. O overhead e minimo — uma classe estatica
  com 3-5 linhas.
- **Ordem de chamada**: O Composition Root precisa chamar os
  Bootstrappers na ordem correta (Configuration antes de PostgreSql,
  por exemplo). Na pratica, `TryAdd*` torna a ordem irrelevante para
  registro — a ordem so importa se houver dependencias de
  inicializacao.

## Fundamentacao Teorica

### Padroes de Design Relacionados

- **Composition Root** (Mark Seemann): O ponto unico onde todas as
  dependencias sao compostas. Nosso padrao respeita isso — o
  Composition Root existe, mas delega para Bootstrappers por camada.
- **Modular Registration** (autofac, Microsoft.Extensions.DI):
  Bibliotecas de DI incentivam agrupar registros em modulos. O
  Bootstrapper e nosso modulo canonico.
- **Convention over Configuration** (Rails): O nome `Bootstrapper.cs`,
  a localizacao na raiz e a assinatura `ConfigureServices` sao
  convencoes que eliminam a necessidade de documentar "onde fica o
  registro de IoC de cada camada".

### O Que o Clean Architecture Diz

> "The Main component is the ultimate detail — the lowest-level policy.
> It is the initial entry point of the system. Nothing, other than the
> operating system, depends on it."
>
> *O componente Main e o detalhe final — a politica de mais baixo nivel.
> E o ponto de entrada inicial do sistema. Nada, alem do sistema
> operacional, depende dele.*

Robert C. Martin (2017). O Composition Root (Main) deve ser o unico
lugar que conhece todas as camadas — e nossos Bootstrappers garantem
que esse conhecimento se limita a "quais camadas existem", nao "quais
classes internas cada camada tem".

## Aprenda Mais

### Perguntas Para Fazer a LLM

1. "Onde fica o registro de IoC de uma camada no Bedrock?"
2. "Qual a diferenca entre Bootstrapper de BuildingBlock e de camada
   de aplicacao?"
3. "Como adicionar um novo servico ao IoC de Infra.Data.PostgreSql?"
4. "Qual lifetime usar para Connection vs. UnitOfWork vs. Mapper?"

### Leitura Recomendada

- Mark Seemann, *Dependency Injection: Principles, Practices,
  Patterns* (2019), Cap. 4 — Composition Root
- Robert C. Martin, *Clean Architecture* (2017), Cap. 26 — The Main
  Component

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Bedrock.BuildingBlocks.Configuration | Fornece `Bootstrapper` com `AddBedrockConfiguration<TManager>()` para registro de ConfigurationManagers |
| Bedrock.BuildingBlocks.Persistence.PostgreSql | Define as base classes cujas implementacoes sao registradas nos Bootstrappers de camada |

## Referencias no Codigo

- Bootstrapper de Configuration (BB): `src/BuildingBlocks/Configuration/Bootstrapper.cs`
- Bootstrapper de Configuration (Auth): `src/ShopDemo/Auth/Infra.CrossCutting.Configuration/Bootstrapper.cs`
- Bootstrapper de PostgreSql (Auth): `src/ShopDemo/Auth/Infra.Data.PostgreSql/Bootstrapper.cs`
- ADR relacionada: [IN-001 — Camadas Canonicas](./IN-001-camadas-canonicas-bounded-context.md)
- ADR relacionada: [IN-015 — Estrutura Canonica de Pastas](./IN-015-estrutura-pastas-canonica-infra-data-tech.md)
