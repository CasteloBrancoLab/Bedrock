using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Domain.Repositories.Interfaces;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.RefreshTokens;
using ShopDemo.Auth.Domain.Repositories.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Repositories;

public class IRefreshTokenRepositoryTests : TestBase
{
    public IRefreshTokenRepositoryTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void IRefreshTokenRepository_ShouldExtendIRepository()
    {
        // Arrange
        LogArrange("Obtendo tipo IRefreshTokenRepository");
        var type = typeof(IRefreshTokenRepository);

        // Act
        LogAct("Verificando hierarquia de interfaces");
        bool isAssignable = typeof(IRepository<RefreshToken>).IsAssignableFrom(type);

        // Assert
        LogAssert("Verificando que IRefreshTokenRepository estende IRepository<RefreshToken>");
        isAssignable.ShouldBeTrue();
    }

    [Fact]
    public void IRefreshTokenRepository_ShouldDefineGetByUserIdAsync()
    {
        // Arrange
        LogArrange("Obtendo tipo IRefreshTokenRepository");
        var type = typeof(IRefreshTokenRepository);

        // Act
        LogAct("Buscando metodo GetByUserIdAsync");
        var method = type.GetMethod("GetByUserIdAsync");

        // Assert
        LogAssert("Verificando que metodo existe e retorna Task<IReadOnlyList<RefreshToken>>");
        method.ShouldNotBeNull();
        method.ReturnType.ShouldBe(typeof(Task<IReadOnlyList<RefreshToken>>));
    }

    [Fact]
    public void IRefreshTokenRepository_ShouldDefineGetByTokenHashAsync()
    {
        // Arrange
        LogArrange("Obtendo tipo IRefreshTokenRepository");
        var type = typeof(IRefreshTokenRepository);

        // Act
        LogAct("Buscando metodo GetByTokenHashAsync");
        var method = type.GetMethod("GetByTokenHashAsync");

        // Assert
        LogAssert("Verificando que metodo existe e retorna Task<RefreshToken?>");
        method.ShouldNotBeNull();
        method.ReturnType.ShouldBe(typeof(Task<RefreshToken>));
    }

    [Fact]
    public void IRefreshTokenRepository_ShouldDefineGetActiveByFamilyIdAsync()
    {
        // Arrange
        LogArrange("Obtendo tipo IRefreshTokenRepository");
        var type = typeof(IRefreshTokenRepository);

        // Act
        LogAct("Buscando metodo GetActiveByFamilyIdAsync");
        var method = type.GetMethod("GetActiveByFamilyIdAsync");

        // Assert
        LogAssert("Verificando que metodo existe e retorna Task<IReadOnlyList<RefreshToken>>");
        method.ShouldNotBeNull();
        method.ReturnType.ShouldBe(typeof(Task<IReadOnlyList<RefreshToken>>));
    }

    [Fact]
    public void IRefreshTokenRepository_ShouldDefineUpdateAsync()
    {
        // Arrange
        LogArrange("Obtendo tipo IRefreshTokenRepository");
        var type = typeof(IRefreshTokenRepository);

        // Act
        LogAct("Buscando metodo UpdateAsync");
        var method = type.GetMethod("UpdateAsync");

        // Assert
        LogAssert("Verificando que metodo existe e retorna Task<bool>");
        method.ShouldNotBeNull();
        method.ReturnType.ShouldBe(typeof(Task<bool>));
    }
}
