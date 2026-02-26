using System.Text;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Services;
using ShopDemo.Auth.Domain.Services.Interfaces;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace ShopDemo.UnitTests.Auth.Domain.Services;

public class RequestSigningServiceTests : TestBase
{
    private readonly RequestSigningService _sut;

    public RequestSigningServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _sut = new RequestSigningService();
    }

    #region Interface Implementation

    [Fact]
    public void ShouldImplementIRequestSigningService()
    {
        LogAssert("Verifying interface implementation");
        _sut.ShouldBeAssignableTo<IRequestSigningService>();
    }

    #endregion

    #region ComputeSignature Tests

    [Fact]
    public void ComputeSignature_ShouldReturnHexString()
    {
        // Arrange
        LogArrange("Preparing request body and secret");
        var requestBody = Encoding.UTF8.GetBytes("test-request-body");
        var sharedSecret = Encoding.UTF8.GetBytes("test-shared-secret");

        // Act
        LogAct("Computing signature");
        var signature = _sut.ComputeSignature(requestBody, sharedSecret);

        // Assert
        LogAssert("Verifying signature is 64-char hex string (HMAC-SHA256)");
        signature.ShouldNotBeNullOrWhiteSpace();
        signature.Length.ShouldBe(64);
    }

    [Fact]
    public void ComputeSignature_ShouldBeDeterministic()
    {
        // Arrange
        LogArrange("Preparing request body and secret");
        var requestBody = Encoding.UTF8.GetBytes("test-request-body");
        var sharedSecret = Encoding.UTF8.GetBytes("test-shared-secret");

        // Act
        LogAct("Computing signature twice");
        var sig1 = _sut.ComputeSignature(requestBody, sharedSecret);
        var sig2 = _sut.ComputeSignature(requestBody, sharedSecret);

        // Assert
        LogAssert("Verifying signatures are identical");
        sig1.ShouldBe(sig2);
    }

    [Fact]
    public void ComputeSignature_WithDifferentBodies_ShouldReturnDifferentSignatures()
    {
        // Arrange
        LogArrange("Preparing different request bodies with same secret");
        var sharedSecret = Encoding.UTF8.GetBytes("test-shared-secret");
        var body1 = Encoding.UTF8.GetBytes("body-1");
        var body2 = Encoding.UTF8.GetBytes("body-2");

        // Act
        LogAct("Computing signatures for different bodies");
        var sig1 = _sut.ComputeSignature(body1, sharedSecret);
        var sig2 = _sut.ComputeSignature(body2, sharedSecret);

        // Assert
        LogAssert("Verifying signatures are different");
        sig1.ShouldNotBe(sig2);
    }

    #endregion

    #region ValidateSignature Tests

    [Fact]
    public void ValidateSignature_WithCorrectSignature_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Computing signature for test body");
        var requestBody = Encoding.UTF8.GetBytes("test-request-body");
        var sharedSecret = Encoding.UTF8.GetBytes("test-shared-secret");
        var signature = _sut.ComputeSignature(requestBody, sharedSecret);

        // Act
        LogAct("Validating correct signature");
        var result = _sut.ValidateSignature(requestBody, sharedSecret, signature);

        // Assert
        LogAssert("Verifying validation returns true");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateSignature_WithIncorrectSignature_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Preparing request with wrong signature");
        var requestBody = Encoding.UTF8.GetBytes("test-request-body");
        var sharedSecret = Encoding.UTF8.GetBytes("test-shared-secret");
        var wrongSignature = _sut.ComputeSignature(Encoding.UTF8.GetBytes("different-body"), sharedSecret);

        // Act
        LogAct("Validating incorrect signature");
        var result = _sut.ValidateSignature(requestBody, sharedSecret, wrongSignature);

        // Assert
        LogAssert("Verifying validation returns false");
        result.ShouldBeFalse();
    }

    [Fact]
    public void ValidateSignature_WithDifferentSecret_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Computing signature with one secret, validating with another");
        var requestBody = Encoding.UTF8.GetBytes("test-request-body");
        var secret1 = Encoding.UTF8.GetBytes("secret-1");
        var secret2 = Encoding.UTF8.GetBytes("secret-2");
        var signature = _sut.ComputeSignature(requestBody, secret1);

        // Act
        LogAct("Validating with different secret");
        var result = _sut.ValidateSignature(requestBody, secret2, signature);

        // Assert
        LogAssert("Verifying validation returns false");
        result.ShouldBeFalse();
    }

    #endregion
}
