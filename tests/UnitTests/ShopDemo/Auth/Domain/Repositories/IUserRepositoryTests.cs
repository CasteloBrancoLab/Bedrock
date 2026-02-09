using Bedrock.BuildingBlocks.Domain.Repositories;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Domain.Repositories;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Repositories;

public class IUserRepositoryTests : TestBase
{
    public IUserRepositoryTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void IUserRepository_ShouldExtendIRepository()
    {
        // Arrange
        LogArrange("Getting IUserRepository type");
        var type = typeof(IUserRepository);

        // Act
        LogAct("Checking if extends IRepository<User>");
        bool extendsIRepository = typeof(IRepository<User>).IsAssignableFrom(type);

        // Assert
        LogAssert("Verifying interface hierarchy");
        extendsIRepository.ShouldBeTrue();
    }

    [Fact]
    public void IUserRepository_ShouldDefineGetByEmailAsync()
    {
        // Arrange
        LogArrange("Getting IUserRepository type");
        var type = typeof(IUserRepository);

        // Act
        LogAct("Checking for GetByEmailAsync method");
        var method = type.GetMethod("GetByEmailAsync");

        // Assert
        LogAssert("Verifying method exists");
        method.ShouldNotBeNull();
        method.ReturnType.ShouldBe(typeof(Task<User?>));
    }

    [Fact]
    public void IUserRepository_ShouldDefineGetByUsernameAsync()
    {
        // Arrange
        LogArrange("Getting IUserRepository type");
        var type = typeof(IUserRepository);

        // Act
        LogAct("Checking for GetByUsernameAsync method");
        var method = type.GetMethod("GetByUsernameAsync");

        // Assert
        LogAssert("Verifying method exists");
        method.ShouldNotBeNull();
        method.ReturnType.ShouldBe(typeof(Task<User?>));
    }

    [Fact]
    public void IUserRepository_ShouldDefineExistsByEmailAsync()
    {
        // Arrange
        LogArrange("Getting IUserRepository type");
        var type = typeof(IUserRepository);

        // Act
        LogAct("Checking for ExistsByEmailAsync method");
        var method = type.GetMethod("ExistsByEmailAsync");

        // Assert
        LogAssert("Verifying method exists");
        method.ShouldNotBeNull();
        method.ReturnType.ShouldBe(typeof(Task<bool>));
    }

    [Fact]
    public void IUserRepository_ShouldDefineExistsByUsernameAsync()
    {
        // Arrange
        LogArrange("Getting IUserRepository type");
        var type = typeof(IUserRepository);

        // Act
        LogAct("Checking for ExistsByUsernameAsync method");
        var method = type.GetMethod("ExistsByUsernameAsync");

        // Assert
        LogAssert("Verifying method exists");
        method.ShouldNotBeNull();
        method.ReturnType.ShouldBe(typeof(Task<bool>));
    }
}
