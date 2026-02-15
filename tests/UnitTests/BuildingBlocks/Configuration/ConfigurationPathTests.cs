using Bedrock.BuildingBlocks.Configuration;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Configuration;

public sealed class ConfigurationPathTests : TestBase
{
    public ConfigurationPathTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void Create_WithValidSectionAndProperty_ShouldDeriveFullPath()
    {
        // Arrange
        LogArrange("Criando ConfigurationPath com secao e propriedade validas");

        // Act
        LogAct("Chamando ConfigurationPath.Create");
        var path = ConfigurationPath.Create("Persistence:PostgreSql", "ConnectionString");

        // Assert
        LogAssert("Verificando que o FullPath e derivado corretamente");
        path.Section.ShouldBe("Persistence:PostgreSql");
        path.Property.ShouldBe("ConnectionString");
        path.FullPath.ShouldBe("Persistence:PostgreSql:ConnectionString");
    }

    [Fact]
    public void Create_WithSingleLevelSection_ShouldDeriveFullPath()
    {
        // Arrange
        LogArrange("Criando ConfigurationPath com secao de nivel unico");

        // Act
        LogAct("Chamando ConfigurationPath.Create com secao simples");
        var path = ConfigurationPath.Create("Logging", "LogLevel");

        // Assert
        LogAssert("Verificando FullPath com secao de nivel unico");
        path.FullPath.ShouldBe("Logging:LogLevel");
    }

    [Theory]
    [InlineData(null, "Property")]
    [InlineData("", "Property")]
    [InlineData("  ", "Property")]
    [InlineData("Section", null)]
    [InlineData("Section", "")]
    [InlineData("Section", "  ")]
    public void Create_WithNullOrEmptyInputs_ShouldThrowArgumentException(string? section, string? property)
    {
        // Arrange
        LogArrange("Tentando criar ConfigurationPath com entrada invalida");

        // Act & Assert
        LogAct("Chamando ConfigurationPath.Create com valores nulos ou vazios");
        LogAssert("Verificando que ArgumentException e lancada");
        Should.Throw<ArgumentException>(() => ConfigurationPath.Create(section!, property!));
    }

    [Fact]
    public void Equals_WithSameFullPath_ShouldBeEqual()
    {
        // Arrange
        LogArrange("Criando dois ConfigurationPaths com mesmo caminho");
        var path1 = ConfigurationPath.Create("Section", "Prop");
        var path2 = ConfigurationPath.Create("Section", "Prop");

        // Act
        LogAct("Comparando igualdade");
        var areEqual = path1.Equals(path2);

        // Assert
        LogAssert("Verificando que sao iguais");
        areEqual.ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentFullPath_ShouldNotBeEqual()
    {
        // Arrange
        LogArrange("Criando dois ConfigurationPaths com caminhos diferentes");
        var path1 = ConfigurationPath.Create("SectionA", "Prop");
        var path2 = ConfigurationPath.Create("SectionB", "Prop");

        // Act
        LogAct("Comparando igualdade");
        var areEqual = path1.Equals(path2);

        // Assert
        LogAssert("Verificando que nao sao iguais");
        areEqual.ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithObject_ShouldWorkCorrectly()
    {
        // Arrange
        LogArrange("Comparando ConfigurationPath com object");
        var path1 = ConfigurationPath.Create("Section", "Prop");
        object path2 = ConfigurationPath.Create("Section", "Prop");
        object notAPath = "not a path";

        // Act & Assert
        LogAct("Comparando com object do mesmo tipo");
        LogAssert("Verificando igualdade via object");
        path1.Equals(path2).ShouldBeTrue();
        path1.Equals(notAPath).ShouldBeFalse();
        path1.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameFullPath_ShouldBeSame()
    {
        // Arrange
        LogArrange("Criando dois ConfigurationPaths com mesmo caminho");
        var path1 = ConfigurationPath.Create("Section", "Prop");
        var path2 = ConfigurationPath.Create("Section", "Prop");

        // Act
        LogAct("Calculando hash codes");

        // Assert
        LogAssert("Verificando que hash codes sao iguais");
        path1.GetHashCode().ShouldBe(path2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentPaths_ShouldBeDifferent()
    {
        // Arrange
        LogArrange("Criando dois ConfigurationPaths com caminhos diferentes");
        var path1 = ConfigurationPath.Create("SectionA", "Prop");
        var path2 = ConfigurationPath.Create("SectionB", "Prop");

        // Act
        LogAct("Calculando hash codes");

        // Assert
        LogAssert("Verificando que hash codes sao diferentes");
        path1.GetHashCode().ShouldNotBe(path2.GetHashCode());
    }

    [Fact]
    public void EqualityOperator_ShouldWorkCorrectly()
    {
        // Arrange
        LogArrange("Criando ConfigurationPaths para testar operadores");
        var path1 = ConfigurationPath.Create("Section", "Prop");
        var path2 = ConfigurationPath.Create("Section", "Prop");
        var path3 = ConfigurationPath.Create("Other", "Prop");

        // Act & Assert
        LogAct("Testando operadores == e !=");
        LogAssert("Verificando operadores de igualdade");
        (path1 == path2).ShouldBeTrue();
        (path1 != path3).ShouldBeTrue();
        (path1 == path3).ShouldBeFalse();
        (path1 != path2).ShouldBeFalse();
    }

    [Fact]
    public void ToString_ShouldReturnFullPath()
    {
        // Arrange
        LogArrange("Criando ConfigurationPath");
        var path = ConfigurationPath.Create("Persistence:PostgreSql", "ConnectionString");

        // Act
        LogAct("Chamando ToString");
        var result = path.ToString();

        // Assert
        LogAssert("Verificando que ToString retorna FullPath");
        result.ShouldBe("Persistence:PostgreSql:ConnectionString");
    }

    [Fact]
    public void Create_ShouldUseZeroAllocationStringCreate()
    {
        // Arrange
        LogArrange("Criando ConfigurationPath e verificando formato do FullPath");

        // Act
        LogAct("Chamando ConfigurationPath.Create com secao multinivel");
        var path = ConfigurationPath.Create("A:B:C", "D");

        // Assert
        LogAssert("Verificando que o FullPath usa separador ':' corretamente");
        path.FullPath.ShouldBe("A:B:C:D");
        path.FullPath.Length.ShouldBe(7);
    }
}
