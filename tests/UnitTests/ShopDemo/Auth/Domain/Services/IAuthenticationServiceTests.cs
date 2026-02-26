using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class IAuthenticationServiceTests : TestBase
{
    public IAuthenticationServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void IAuthenticationService_ShouldDefineRegisterUserAsync()
    {
        // Arrange
        LogArrange("Getting IAuthenticationService type");
        var type = typeof(IAuthenticationService);

        // Act
        LogAct("Checking for RegisterUserAsync method");
        var method = type.GetMethod("RegisterUserAsync");

        // Assert
        LogAssert("Verifying method exists and returns Task<User?>");
        method.ShouldNotBeNull();
        method.ReturnType.ShouldBe(typeof(Task<User?>));
    }

    [Fact]
    public void IAuthenticationService_ShouldDefineVerifyCredentialsAsync()
    {
        // Arrange
        LogArrange("Getting IAuthenticationService type");
        var type = typeof(IAuthenticationService);

        // Act
        LogAct("Checking for VerifyCredentialsAsync method");
        var method = type.GetMethod("VerifyCredentialsAsync");

        // Assert
        LogAssert("Verifying method exists and returns Task<User?>");
        method.ShouldNotBeNull();
        method.ReturnType.ShouldBe(typeof(Task<User?>));
    }
}
