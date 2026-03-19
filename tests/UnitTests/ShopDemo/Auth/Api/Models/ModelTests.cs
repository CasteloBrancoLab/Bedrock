using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Api.Controllers.V1.Auth.Models;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Api.Models;

public class ModelTests : TestBase
{
    public ModelTests(ITestOutputHelper output) : base(output) { }

    #region RegisterPayload

    [Fact]
    public void RegisterPayload_ShouldStoreProperties()
    {
        LogAct("Creating RegisterPayload");
        var sut = new RegisterPayload("test@example.com", "SecurePassword123!");

        LogAssert("Verifying properties");
        sut.Email.ShouldBe("test@example.com");
        sut.Password.ShouldBe("SecurePassword123!");
    }

    [Fact]
    public void RegisterPayload_Equality_ShouldWorkByValue()
    {
        LogAct("Creating two identical RegisterPayload instances");
        var a = new RegisterPayload("a@b.com", "pass");
        var b = new RegisterPayload("a@b.com", "pass");

        LogAssert("Verifying value equality");
        a.ShouldBe(b);
    }

    #endregion

    #region RegisterResponse

    [Fact]
    public void RegisterResponse_ShouldStoreProperties()
    {
        LogAct("Creating RegisterResponse");
        var id = Guid.NewGuid();
        var sut = new RegisterResponse(id, "test@example.com");

        LogAssert("Verifying properties");
        sut.UserId.ShouldBe(id);
        sut.Email.ShouldBe("test@example.com");
    }

    [Fact]
    public void RegisterResponse_Equality_ShouldWorkByValue()
    {
        LogAct("Creating two identical RegisterResponse instances");
        var id = Guid.NewGuid();
        var a = new RegisterResponse(id, "a@b.com");
        var b = new RegisterResponse(id, "a@b.com");

        LogAssert("Verifying value equality");
        a.ShouldBe(b);
    }

    #endregion

    #region LoginPayload

    [Fact]
    public void LoginPayload_ShouldStoreProperties()
    {
        LogAct("Creating LoginPayload");
        var sut = new LoginPayload("test@example.com", "SecurePassword123!");

        LogAssert("Verifying properties");
        sut.Email.ShouldBe("test@example.com");
        sut.Password.ShouldBe("SecurePassword123!");
    }

    [Fact]
    public void LoginPayload_Equality_ShouldWorkByValue()
    {
        LogAct("Creating two identical LoginPayload instances");
        var a = new LoginPayload("a@b.com", "pass");
        var b = new LoginPayload("a@b.com", "pass");

        LogAssert("Verifying value equality");
        a.ShouldBe(b);
    }

    #endregion

    #region LoginResponse

    [Fact]
    public void LoginResponse_ShouldStoreProperties()
    {
        LogAct("Creating LoginResponse");
        var id = Guid.NewGuid();
        var sut = new LoginResponse(id, "test@example.com");

        LogAssert("Verifying properties");
        sut.UserId.ShouldBe(id);
        sut.Email.ShouldBe("test@example.com");
    }

    [Fact]
    public void LoginResponse_Equality_ShouldWorkByValue()
    {
        LogAct("Creating two identical LoginResponse instances");
        var id = Guid.NewGuid();
        var a = new LoginResponse(id, "a@b.com");
        var b = new LoginResponse(id, "a@b.com");

        LogAssert("Verifying value equality");
        a.ShouldBe(b);
    }

    #endregion
}
