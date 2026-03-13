using Bedrock.BuildingBlocks.Testing;
using Bedrock.BuildingBlocks.Web.WebApi.Models;
using ShopDemo.Auth.Api.Models;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Api.Models;

public class ModelTests : TestBase
{
    public ModelTests(ITestOutputHelper output) : base(output) { }

    #region ErrorResponse

    [Fact]
    public void ErrorResponse_ShouldStoreProperties()
    {
        LogAct("Creating ErrorResponse");
        var sut = new ErrorResponse("ERR_CODE", "Something went wrong");

        LogAssert("Verifying properties");
        sut.Code.ShouldBe("ERR_CODE");
        sut.Message.ShouldBe("Something went wrong");
    }

    [Fact]
    public void ErrorResponse_Equality_ShouldWorkByValue()
    {
        LogAct("Creating two identical ErrorResponse instances");
        var a = new ErrorResponse("ERR", "msg");
        var b = new ErrorResponse("ERR", "msg");

        LogAssert("Verifying value equality");
        a.ShouldBe(b);
    }

    #endregion

    #region RegisterRequest

    [Fact]
    public void RegisterRequest_ShouldStoreProperties()
    {
        LogAct("Creating RegisterRequest");
        var sut = new RegisterRequest("test@example.com", "SecurePassword123!");

        LogAssert("Verifying properties");
        sut.Email.ShouldBe("test@example.com");
        sut.Password.ShouldBe("SecurePassword123!");
    }

    [Fact]
    public void RegisterRequest_Equality_ShouldWorkByValue()
    {
        LogAct("Creating two identical RegisterRequest instances");
        var a = new RegisterRequest("a@b.com", "pass");
        var b = new RegisterRequest("a@b.com", "pass");

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

    #region LoginRequest

    [Fact]
    public void LoginRequest_ShouldStoreProperties()
    {
        LogAct("Creating LoginRequest");
        var sut = new LoginRequest("test@example.com", "SecurePassword123!");

        LogAssert("Verifying properties");
        sut.Email.ShouldBe("test@example.com");
        sut.Password.ShouldBe("SecurePassword123!");
    }

    [Fact]
    public void LoginRequest_Equality_ShouldWorkByValue()
    {
        LogAct("Creating two identical LoginRequest instances");
        var a = new LoginRequest("a@b.com", "pass");
        var b = new LoginRequest("a@b.com", "pass");

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
