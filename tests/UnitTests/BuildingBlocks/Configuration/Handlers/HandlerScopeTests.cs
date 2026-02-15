using Bedrock.BuildingBlocks.Configuration.Handlers;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Configuration.Handlers;

public sealed class HandlerScopeTests : TestBase
{
    public HandlerScopeTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void Global_ShouldMatchAllKeys()
    {
        // Arrange
        LogArrange("Criando HandlerScope global");
        var scope = HandlerScope.Global();

        // Act & Assert
        LogAct("Verificando matching com diversas chaves");
        LogAssert("Verificando que global corresponde a todas as chaves");
        scope.Matches("Persistence:PostgreSql:ConnectionString").ShouldBeTrue();
        scope.Matches("Security:Jwt:Secret").ShouldBeTrue();
        scope.Matches("AnyKey").ShouldBeTrue();
    }

    [Fact]
    public void Global_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        LogArrange("Criando HandlerScope global");
        LogAct("Verificando propriedades do escopo global");
        var scope = HandlerScope.Global();

        // Assert
        LogAssert("Verificando ScopeType e PathPattern");
        scope.ScopeType.ShouldBe(ScopeType.Global);
        scope.PathPattern.ShouldBe(string.Empty);
    }

    [Fact]
    public void ForClass_ShouldMatchKeysWithSectionPrefix()
    {
        // Arrange
        LogArrange("Criando HandlerScope para classe Persistence:PostgreSql");
        var scope = HandlerScope.ForClass("Persistence:PostgreSql");

        // Act & Assert
        LogAct("Verificando matching com chaves da secao");
        LogAssert("Verificando que corresponde a chaves com prefixo da secao");
        scope.Matches("Persistence:PostgreSql:ConnectionString").ShouldBeTrue();
        scope.Matches("Persistence:PostgreSql:Port").ShouldBeTrue();
        scope.Matches("Persistence:PostgreSql:Schema").ShouldBeTrue();
    }

    [Fact]
    public void ForClass_ShouldNotMatchKeysOutsideSection()
    {
        // Arrange
        LogArrange("Criando HandlerScope para classe Persistence:PostgreSql");
        var scope = HandlerScope.ForClass("Persistence:PostgreSql");

        // Act & Assert
        LogAct("Verificando que nao corresponde a chaves fora da secao");
        LogAssert("Verificando que chaves de outra secao nao correspondem");
        scope.Matches("Security:Jwt:Secret").ShouldBeFalse();
        scope.Matches("Persistence:MySql:ConnectionString").ShouldBeFalse();
    }

    [Fact]
    public void ForClass_ShouldNotMatchSectionPathItself()
    {
        // Arrange
        LogArrange("Criando HandlerScope para classe Persistence:PostgreSql");
        var scope = HandlerScope.ForClass("Persistence:PostgreSql");

        // Act & Assert
        LogAct("Verificando que o caminho da secao em si nao corresponde");
        LogAssert("Verificando que a secao pura nao e match");
        scope.Matches("Persistence:PostgreSql").ShouldBeFalse();
    }

    [Fact]
    public void ForClass_ShouldNotMatchPartialPrefix()
    {
        // Arrange
        LogArrange("Criando HandlerScope para classe Persistence:Pg");
        var scope = HandlerScope.ForClass("Persistence:Pg");

        // Act & Assert
        LogAct("Verificando que prefixo parcial sem separador nao corresponde");
        LogAssert("Verificando que Persistence:PostgreSql nao corresponde a Persistence:Pg");
        scope.Matches("Persistence:PostgreSql:ConnectionString").ShouldBeFalse();
    }

    [Fact]
    public void ForClass_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        LogArrange("Criando HandlerScope para classe");
        LogAct("Verificando propriedades");
        var scope = HandlerScope.ForClass("Persistence:PostgreSql");

        // Assert
        LogAssert("Verificando ScopeType e PathPattern");
        scope.ScopeType.ShouldBe(ScopeType.Class);
        scope.PathPattern.ShouldBe("Persistence:PostgreSql");
    }

    [Fact]
    public void ForProperty_ShouldMatchExactKey()
    {
        // Arrange
        LogArrange("Criando HandlerScope para propriedade exata");
        var scope = HandlerScope.ForProperty("Persistence:PostgreSql:ConnectionString");

        // Act & Assert
        LogAct("Verificando matching com chave exata");
        LogAssert("Verificando que corresponde apenas a chave exata");
        scope.Matches("Persistence:PostgreSql:ConnectionString").ShouldBeTrue();
    }

    [Fact]
    public void ForProperty_ShouldNotMatchDifferentKey()
    {
        // Arrange
        LogArrange("Criando HandlerScope para propriedade exata");
        var scope = HandlerScope.ForProperty("Persistence:PostgreSql:ConnectionString");

        // Act & Assert
        LogAct("Verificando que nao corresponde a chaves diferentes");
        LogAssert("Verificando que chaves diferentes nao correspondem");
        scope.Matches("Persistence:PostgreSql:Port").ShouldBeFalse();
        scope.Matches("Persistence:PostgreSql:ConnectionString:Extra").ShouldBeFalse();
        scope.Matches("Security:Jwt:Secret").ShouldBeFalse();
    }

    [Fact]
    public void ForProperty_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        LogArrange("Criando HandlerScope para propriedade");
        LogAct("Verificando propriedades");
        var scope = HandlerScope.ForProperty("Persistence:PostgreSql:ConnectionString");

        // Assert
        LogAssert("Verificando ScopeType e PathPattern");
        scope.ScopeType.ShouldBe(ScopeType.Property);
        scope.PathPattern.ShouldBe("Persistence:PostgreSql:ConnectionString");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ForClass_WithNullOrEmptyInput_ShouldThrowArgumentException(string? sectionPath)
    {
        // Arrange
        LogArrange("Tentando criar ForClass com entrada invalida");

        // Act & Assert
        LogAct("Chamando ForClass com valor nulo ou vazio");
        LogAssert("Verificando que ArgumentException e lancada");
        Should.Throw<ArgumentException>(() => HandlerScope.ForClass(sectionPath!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ForProperty_WithNullOrEmptyInput_ShouldThrowArgumentException(string? fullPath)
    {
        // Arrange
        LogArrange("Tentando criar ForProperty com entrada invalida");

        // Act & Assert
        LogAct("Chamando ForProperty com valor nulo ou vazio");
        LogAssert("Verificando que ArgumentException e lancada");
        Should.Throw<ArgumentException>(() => HandlerScope.ForProperty(fullPath!));
    }

    [Fact]
    public void Equals_WithSameScope_ShouldBeEqual()
    {
        // Arrange
        LogArrange("Criando dois HandlerScopes identicos");
        var scope1 = HandlerScope.ForClass("Section");
        var scope2 = HandlerScope.ForClass("Section");

        // Act & Assert
        LogAct("Comparando igualdade");
        LogAssert("Verificando que sao iguais");
        scope1.Equals(scope2).ShouldBeTrue();
        (scope1 == scope2).ShouldBeTrue();
        (scope1 != scope2).ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithDifferentScope_ShouldNotBeEqual()
    {
        // Arrange
        LogArrange("Criando HandlerScopes diferentes");
        var scope1 = HandlerScope.ForClass("SectionA");
        var scope2 = HandlerScope.ForClass("SectionB");
        var scope3 = HandlerScope.Global();

        // Act & Assert
        LogAct("Comparando desigualdade");
        LogAssert("Verificando que sao diferentes");
        scope1.Equals(scope2).ShouldBeFalse();
        scope1.Equals(scope3).ShouldBeFalse();
        (scope1 != scope2).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithObject_ShouldWorkCorrectly()
    {
        // Arrange
        LogArrange("Comparando HandlerScope com object");
        var scope1 = HandlerScope.Global();
        object scope2 = HandlerScope.Global();
        object notAScope = "not a scope";

        // Act & Assert
        LogAct("Comparando com object");
        LogAssert("Verificando igualdade via object");
        scope1.Equals(scope2).ShouldBeTrue();
        scope1.Equals(notAScope).ShouldBeFalse();
        scope1.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameScope_ShouldBeSame()
    {
        // Arrange
        LogArrange("Criando dois HandlerScopes identicos");
        var scope1 = HandlerScope.ForProperty("Key");
        var scope2 = HandlerScope.ForProperty("Key");

        // Act & Assert
        LogAct("Calculando hash codes");
        LogAssert("Verificando que hash codes sao iguais");
        scope1.GetHashCode().ShouldBe(scope2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentScope_ShouldBeDifferent()
    {
        // Arrange
        LogArrange("Criando HandlerScopes diferentes");
        var scope1 = HandlerScope.ForProperty("KeyA");
        var scope2 = HandlerScope.ForProperty("KeyB");

        // Act & Assert
        LogAct("Calculando hash codes");
        LogAssert("Verificando que hash codes sao diferentes");
        scope1.GetHashCode().ShouldNotBe(scope2.GetHashCode());
    }

    [Fact]
    public void BothKeyExactAndSectionPrefix_MatchSameKey()
    {
        // Arrange
        LogArrange("Criando escopos de chave exata e prefixo de secao que correspondem a mesma chave");
        var exactScope = HandlerScope.ForProperty("Persistence:PostgreSql:ConnectionString");
        var classScope = HandlerScope.ForClass("Persistence:PostgreSql");
        var globalScope = HandlerScope.Global();

        // Act & Assert
        LogAct("Verificando que todos correspondem a mesma chave");
        LogAssert("Verificando que chave exata, classe e global todos correspondem");
        var key = "Persistence:PostgreSql:ConnectionString";
        exactScope.Matches(key).ShouldBeTrue();
        classScope.Matches(key).ShouldBeTrue();
        globalScope.Matches(key).ShouldBeTrue();
    }
}
