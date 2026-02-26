---
paths:
  - "tests/UnitTests/**"
---

# Convencoes de Testes Unitarios

## Padrao Obrigatorio: AAA (Arrange, Act, Assert)

```csharp
[Fact]
public void MyTest()
{
    // Arrange
    LogArrange("Preparando dados");

    // Act
    LogAct("Executando acao");

    // Assert
    LogAssert("Verificando resultado");
}
```

## Classe Base

Todos os testes herdam de `TestBase`:

```csharp
public class MyTests : TestBase
{
    public MyTests(ITestOutputHelper output) : base(output) { }
}
```

Para testes com IoC, usar `ServiceCollectionFixture`:

```csharp
public class MyFixture : ServiceCollectionFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IMyService, MyService>();
    }
}

[CollectionDefinition("MyServices")]
public class MyCollection : ICollectionFixture<MyFixture> { }

[Collection("MyServices")]
public class MyTests : TestBase
{
    private readonly MyFixture _fixture;
    public MyTests(MyFixture fixture, ITestOutputHelper output) : base(output)
    {
        _fixture = fixture;
    }
}
```

## Bibliotecas

| Biblioteca | Proposito |
|------------|-----------|
| xUnit | Framework de testes |
| Shouldly | Assertions fluentes |
| Moq | Mocking |
| Bogus | Geracao de dados fake |
| Humanizer | Formatacao humanizada de logs |

## Nomenclatura

- Relacao **1:1** entre projeto `src` e projeto `tests`
- Formato: `Bedrock.UnitTests.<namespace-do-src>`
- Compatibilidade com Stryker.NET (mutation testing)
