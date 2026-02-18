using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Bedrock.BuildingBlocks.Testing;
using ShopDemo.Auth.Domain.Entities.ServiceClients;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Enums;
using ShopDemo.Auth.Domain.Entities.ServiceClients.Inputs;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

using ExecutionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext;
using ServiceClientMetadata = ShopDemo.Auth.Domain.Entities.ServiceClients.ServiceClient.ServiceClientMetadata;

namespace ShopDemo.UnitTests.Auth.Domain.Entities.ServiceClients;

public class ServiceClientTests : TestBase
{
    public ServiceClientTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region RegisterNew Tests

    [Fact]
    public void RegisterNew_WithValidInput_ShouldCreateServiceClient()
    {
        // Arrange
        LogArrange("Creating execution context and input");
        var executionContext = CreateTestExecutionContext();
        var createdByUserId = Id.GenerateNewId();
        byte[] secretHash = CreateValidClientSecretHash();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(365);
        var input = new RegisterNewServiceClientInput(
            "my-service-client", secretHash, "My Service", createdByUserId, expiresAt);

        // Act
        LogAct("Registering new service client");
        var client = ServiceClient.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying service client was created successfully");
        client.ShouldNotBeNull();
        client.ClientId.ShouldBe("my-service-client");
        client.ClientSecretHash.ShouldBe(secretHash);
        client.Name.ShouldBe("My Service");
        client.Status.ShouldBe(ServiceClientStatus.Active);
        client.CreatedByUserId.ShouldBe(createdByUserId);
        client.ExpiresAt.ShouldBe(expiresAt);
        client.RevokedAt.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_ShouldAlwaysSetStatusToActive()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new service client");
        var client = ServiceClient.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying status is Active");
        client.ShouldNotBeNull();
        client.Status.ShouldBe(ServiceClientStatus.Active);
    }

    [Fact]
    public void RegisterNew_ShouldSetRevokedAtToNull()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new service client");
        var client = ServiceClient.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying RevokedAt is null");
        client.ShouldNotBeNull();
        client.RevokedAt.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_ShouldAssignEntityInfo()
    {
        // Arrange
        LogArrange("Creating valid input");
        var executionContext = CreateTestExecutionContext();
        var input = CreateValidRegisterNewInput();

        // Act
        LogAct("Registering new service client");
        var client = ServiceClient.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying EntityInfo is assigned");
        client.ShouldNotBeNull();
        client.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void RegisterNew_WithNullExpiresAt_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating input with null ExpiresAt");
        var executionContext = CreateTestExecutionContext();
        var createdByUserId = Id.GenerateNewId();
        var input = new RegisterNewServiceClientInput(
            "my-client", CreateValidClientSecretHash(), "My Service",
            createdByUserId, null);

        // Act
        LogAct("Registering new service client with null ExpiresAt");
        var client = ServiceClient.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying service client was created with null ExpiresAt");
        client.ShouldNotBeNull();
        client.ExpiresAt.ShouldBeNull();
    }

    [Fact]
    public void RegisterNew_WithNullClientId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null ClientId");
        var executionContext = CreateTestExecutionContext();
        var createdByUserId = Id.GenerateNewId();
        var input = new RegisterNewServiceClientInput(
            null!, CreateValidClientSecretHash(), "My Service",
            createdByUserId, null);

        // Act
        LogAct("Registering new service client with null ClientId");
        var client = ServiceClient.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        client.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithEmptyClientId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty ClientId");
        var executionContext = CreateTestExecutionContext();
        var createdByUserId = Id.GenerateNewId();
        var input = new RegisterNewServiceClientInput(
            "", CreateValidClientSecretHash(), "My Service",
            createdByUserId, null);

        // Act
        LogAct("Registering new service client with empty ClientId");
        var client = ServiceClient.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        client.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithClientIdExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with ClientId exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string longClientId = new('a', ServiceClientMetadata.ClientIdMaxLength + 1);
        var createdByUserId = Id.GenerateNewId();
        var input = new RegisterNewServiceClientInput(
            longClientId, CreateValidClientSecretHash(), "My Service",
            createdByUserId, null);

        // Act
        LogAct("Registering new service client with too-long ClientId");
        var client = ServiceClient.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        client.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithNullClientSecretHash_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null ClientSecretHash");
        var executionContext = CreateTestExecutionContext();
        var createdByUserId = Id.GenerateNewId();
        var input = new RegisterNewServiceClientInput(
            "my-client", null!, "My Service", createdByUserId, null);

        // Act
        LogAct("Registering new service client with null ClientSecretHash");
        var client = ServiceClient.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        client.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithEmptyClientSecretHash_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty ClientSecretHash");
        var executionContext = CreateTestExecutionContext();
        var createdByUserId = Id.GenerateNewId();
        var input = new RegisterNewServiceClientInput(
            "my-client", [], "My Service", createdByUserId, null);

        // Act
        LogAct("Registering new service client with empty ClientSecretHash");
        var client = ServiceClient.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        client.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithClientSecretHashExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with ClientSecretHash exceeding max length");
        var executionContext = CreateTestExecutionContext();
        byte[] longHash = new byte[ServiceClientMetadata.ClientSecretHashMaxLength + 1];
        longHash[0] = 1;
        var createdByUserId = Id.GenerateNewId();
        var input = new RegisterNewServiceClientInput(
            "my-client", longHash, "My Service", createdByUserId, null);

        // Act
        LogAct("Registering new service client with too-long ClientSecretHash");
        var client = ServiceClient.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        client.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithNullName_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with null Name");
        var executionContext = CreateTestExecutionContext();
        var createdByUserId = Id.GenerateNewId();
        var input = new RegisterNewServiceClientInput(
            "my-client", CreateValidClientSecretHash(), null!,
            createdByUserId, null);

        // Act
        LogAct("Registering new service client with null Name");
        var client = ServiceClient.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        client.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithEmptyName_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with empty Name");
        var executionContext = CreateTestExecutionContext();
        var createdByUserId = Id.GenerateNewId();
        var input = new RegisterNewServiceClientInput(
            "my-client", CreateValidClientSecretHash(), "",
            createdByUserId, null);

        // Act
        LogAct("Registering new service client with empty Name");
        var client = ServiceClient.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        client.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithNameExceedingMaxLength_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with Name exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string longName = new('a', ServiceClientMetadata.NameMaxLength + 1);
        var createdByUserId = Id.GenerateNewId();
        var input = new RegisterNewServiceClientInput(
            "my-client", CreateValidClientSecretHash(), longName,
            createdByUserId, null);

        // Act
        LogAct("Registering new service client with too-long Name");
        var client = ServiceClient.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        client.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterNew_WithDefaultCreatedByUserId_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating input with default CreatedByUserId");
        var executionContext = CreateTestExecutionContext();
        var input = new RegisterNewServiceClientInput(
            "my-client", CreateValidClientSecretHash(), "My Service",
            default(Id), null);

        // Act
        LogAct("Registering new service client with default CreatedByUserId");
        var client = ServiceClient.RegisterNew(executionContext, input);

        // Assert
        LogAssert("Verifying null is returned");
        client.ShouldBeNull();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldCreateServiceClientWithAllProperties()
    {
        // Arrange
        LogArrange("Creating all properties for existing service client");
        var entityInfo = CreateTestEntityInfo();
        var createdByUserId = Id.CreateFromExistingInfo(Guid.NewGuid());
        byte[] secretHash = CreateValidClientSecretHash();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(365);
        var revokedAt = DateTimeOffset.UtcNow;
        var input = new CreateFromExistingInfoServiceClientInput(
            entityInfo, "my-client", secretHash, "My Service",
            ServiceClientStatus.Revoked, createdByUserId, expiresAt, revokedAt);

        // Act
        LogAct("Creating service client from existing info");
        var client = ServiceClient.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying all properties are set");
        client.EntityInfo.ShouldBe(entityInfo);
        client.ClientId.ShouldBe("my-client");
        client.ClientSecretHash.ShouldBe(secretHash);
        client.Name.ShouldBe("My Service");
        client.Status.ShouldBe(ServiceClientStatus.Revoked);
        client.CreatedByUserId.ShouldBe(createdByUserId);
        client.ExpiresAt.ShouldBe(expiresAt);
        client.RevokedAt.ShouldBe(revokedAt);
    }

    [Fact]
    public void CreateFromExistingInfo_ShouldNotValidate()
    {
        // Arrange
        LogArrange("Creating input with empty ClientId (would fail validation)");
        var entityInfo = CreateTestEntityInfo();
        var createdByUserId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new CreateFromExistingInfoServiceClientInput(
            entityInfo, "", [], "", ServiceClientStatus.Active,
            createdByUserId, null, null);

        // Act
        LogAct("Creating service client from existing info with empty ClientId");
        var client = ServiceClient.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying service client was created without validation");
        client.ShouldNotBeNull();
        client.ClientId.ShouldBe("");
    }

    [Fact]
    public void CreateFromExistingInfo_WithNullOptionalFields_ShouldPreserveNull()
    {
        // Arrange
        LogArrange("Creating input with null optional fields");
        var entityInfo = CreateTestEntityInfo();
        var createdByUserId = Id.CreateFromExistingInfo(Guid.NewGuid());
        var input = new CreateFromExistingInfoServiceClientInput(
            entityInfo, "my-client", CreateValidClientSecretHash(), "My Service",
            ServiceClientStatus.Active, createdByUserId, null, null);

        // Act
        LogAct("Creating service client from existing info");
        var client = ServiceClient.CreateFromExistingInfo(input);

        // Assert
        LogAssert("Verifying optional fields are null");
        client.ExpiresAt.ShouldBeNull();
        client.RevokedAt.ShouldBeNull();
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateIdenticalCopy()
    {
        // Arrange
        LogArrange("Creating service client");
        var executionContext = CreateTestExecutionContext();
        var client = CreateTestServiceClient(executionContext);

        // Act
        LogAct("Cloning service client");
        var clone = client.Clone();

        // Assert
        LogAssert("Verifying clone has same values");
        clone.ShouldNotBeNull();
        clone.ShouldNotBeSameAs(client);
        clone.ClientId.ShouldBe(client.ClientId);
        clone.ClientSecretHash.ShouldBe(client.ClientSecretHash);
        clone.Name.ShouldBe(client.Name);
        clone.Status.ShouldBe(client.Status);
        clone.CreatedByUserId.ShouldBe(client.CreatedByUserId);
        clone.ExpiresAt.ShouldBe(client.ExpiresAt);
        clone.RevokedAt.ShouldBe(client.RevokedAt);
    }

    #endregion

    #region Revoke Tests

    [Fact]
    public void Revoke_WithActiveClient_ShouldSucceed()
    {
        // Arrange
        LogArrange("Creating active service client");
        var executionContext = CreateTestExecutionContext();
        var client = CreateTestServiceClient(executionContext);
        var input = new RevokeServiceClientInput();

        // Act
        LogAct("Revoking service client");
        var result = client.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying service client was revoked");
        result.ShouldNotBeNull();
        result.Status.ShouldBe(ServiceClientStatus.Revoked);
        result.RevokedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Revoke_ShouldSetRevokedAtFromExecutionContext()
    {
        // Arrange
        LogArrange("Creating active service client");
        var executionContext = CreateTestExecutionContext();
        var client = CreateTestServiceClient(executionContext);
        var input = new RevokeServiceClientInput();

        // Act
        LogAct("Revoking service client");
        var result = client.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying RevokedAt matches ExecutionContext.Timestamp");
        result.ShouldNotBeNull();
        result.RevokedAt.ShouldBe(executionContext.Timestamp);
    }

    [Fact]
    public void Revoke_ShouldReturnNewInstance()
    {
        // Arrange
        LogArrange("Creating active service client");
        var executionContext = CreateTestExecutionContext();
        var client = CreateTestServiceClient(executionContext);
        var input = new RevokeServiceClientInput();

        // Act
        LogAct("Revoking service client");
        var result = client.Revoke(executionContext, input);

        // Assert
        LogAssert("Verifying clone-modify-return pattern");
        result.ShouldNotBeNull();
        result.ShouldNotBeSameAs(client);
        client.Status.ShouldBe(ServiceClientStatus.Active);
        result.Status.ShouldBe(ServiceClientStatus.Revoked);
    }

    [Fact]
    public void Revoke_WithAlreadyRevokedClient_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating revoked service client");
        var executionContext = CreateTestExecutionContext();
        var client = CreateTestServiceClient(executionContext);
        var revokedClient = client.Revoke(executionContext, new RevokeServiceClientInput())!;
        var input = new RevokeServiceClientInput();

        // Act
        LogAct("Attempting to revoke already revoked service client");
        var newContext = CreateTestExecutionContext();
        var result = revokedClient.Revoke(newContext, input);

        // Assert
        LogAssert("Verifying null is returned for Revoked -> Revoked transition");
        result.ShouldBeNull();
        newContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Instance IsValid (IsValidInternal) Tests

    [Fact]
    public void IsValid_Instance_WithValidClient_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating valid service client");
        var executionContext = CreateTestExecutionContext();
        var client = CreateTestServiceClient(executionContext);

        // Act
        LogAct("Calling instance IsValid to trigger IsValidInternal");
        bool result = client.IsValid(executionContext);

        // Assert
        LogAssert("Verifying instance validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_Instance_WithInvalidClient_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating service client with invalid state via CreateFromExistingInfo");
        var entityInfo = CreateTestEntityInfo();
        var input = new CreateFromExistingInfoServiceClientInput(
            entityInfo, "", CreateValidClientSecretHash(), "Name",
            ServiceClientStatus.Active, Id.GenerateNewId(), null, null);
        var client = ServiceClient.CreateFromExistingInfo(input);
        var validationContext = CreateTestExecutionContext();

        // Act
        LogAct("Calling instance IsValid on client with empty ClientId");
        bool result = client.IsValid(validationContext);

        // Assert
        LogAssert("Verifying instance validation fails for empty ClientId");
        result.ShouldBeFalse();
        validationContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateClientId Tests

    [Fact]
    public void ValidateClientId_WithValidValue_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid ClientId");
        bool result = ServiceClient.ValidateClientId(executionContext, "my-service-client");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateClientId_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null ClientId");
        bool result = ServiceClient.ValidateClientId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateClientId_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty ClientId");
        bool result = ServiceClient.ValidateClientId(executionContext, "");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateClientId_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating ClientId at max length");
        var executionContext = CreateTestExecutionContext();
        string clientId = new('a', ServiceClientMetadata.ClientIdMaxLength);

        // Act
        LogAct("Validating max-length ClientId");
        bool result = ServiceClient.ValidateClientId(executionContext, clientId);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateClientId_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating ClientId exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string clientId = new('a', ServiceClientMetadata.ClientIdMaxLength + 1);

        // Act
        LogAct("Validating too-long ClientId");
        bool result = ServiceClient.ValidateClientId(executionContext, clientId);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateClientSecretHash Tests

    [Fact]
    public void ValidateClientSecretHash_WithValidHash_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid ClientSecretHash");
        bool result = ServiceClient.ValidateClientSecretHash(executionContext, CreateValidClientSecretHash());

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateClientSecretHash_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null ClientSecretHash");
        bool result = ServiceClient.ValidateClientSecretHash(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateClientSecretHash_WithEmptyArray_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty ClientSecretHash");
        bool result = ServiceClient.ValidateClientSecretHash(executionContext, []);

        // Assert
        LogAssert("Verifying validation fails for empty array");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateClientSecretHash_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating ClientSecretHash at max length");
        var executionContext = CreateTestExecutionContext();
        byte[] hash = new byte[ServiceClientMetadata.ClientSecretHashMaxLength];
        hash[0] = 1;

        // Act
        LogAct("Validating max-length ClientSecretHash");
        bool result = ServiceClient.ValidateClientSecretHash(executionContext, hash);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateClientSecretHash_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating ClientSecretHash exceeding max length");
        var executionContext = CreateTestExecutionContext();
        byte[] hash = new byte[ServiceClientMetadata.ClientSecretHashMaxLength + 1];
        hash[0] = 1;

        // Act
        LogAct("Validating too-long ClientSecretHash");
        bool result = ServiceClient.ValidateClientSecretHash(executionContext, hash);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateName Tests

    [Fact]
    public void ValidateName_WithValidName_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating valid Name");
        bool result = ServiceClient.ValidateName(executionContext, "My Service");

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateName_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null Name");
        bool result = ServiceClient.ValidateName(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateName_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating empty Name");
        bool result = ServiceClient.ValidateName(executionContext, "");

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateName_AtMaxLength_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating Name at max length");
        var executionContext = CreateTestExecutionContext();
        string name = new('a', ServiceClientMetadata.NameMaxLength);

        // Act
        LogAct("Validating max-length Name");
        bool result = ServiceClient.ValidateName(executionContext, name);

        // Assert
        LogAssert("Verifying validation passes at boundary");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateName_ExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating Name exceeding max length");
        var executionContext = CreateTestExecutionContext();
        string name = new('a', ServiceClientMetadata.NameMaxLength + 1);

        // Act
        LogAct("Validating too-long Name");
        bool result = ServiceClient.ValidateName(executionContext, name);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateStatus Tests

    [Theory]
    [InlineData(ServiceClientStatus.Active)]
    [InlineData(ServiceClientStatus.Revoked)]
    public void ValidateStatus_WithValidStatus_ShouldReturnTrue(ServiceClientStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating status: {status}");
        bool result = ServiceClient.ValidateStatus(executionContext, status);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateStatus_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null status");
        bool result = ServiceClient.ValidateStatus(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateCreatedByUserId Tests

    [Fact]
    public void ValidateCreatedByUserId_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();
        var userId = Id.GenerateNewId();

        // Act
        LogAct("Validating valid CreatedByUserId");
        bool result = ServiceClient.ValidateCreatedByUserId(executionContext, userId);

        // Assert
        LogAssert("Verifying validation passes");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateCreatedByUserId_WithNull_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null CreatedByUserId");
        bool result = ServiceClient.ValidateCreatedByUserId(executionContext, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateStatusTransition Tests

    [Fact]
    public void ValidateStatusTransition_ActiveToRevoked_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating Active -> Revoked transition");
        bool result = ServiceClient.ValidateStatusTransition(
            executionContext, ServiceClientStatus.Active, ServiceClientStatus.Revoked);

        // Assert
        LogAssert("Verifying transition is valid");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateStatusTransition_RevokedToActive_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating Revoked -> Active transition");
        bool result = ServiceClient.ValidateStatusTransition(
            executionContext, ServiceClientStatus.Revoked, ServiceClientStatus.Active);

        // Assert
        LogAssert("Verifying transition is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Theory]
    [InlineData(ServiceClientStatus.Active)]
    [InlineData(ServiceClientStatus.Revoked)]
    public void ValidateStatusTransition_SameStatus_ShouldReturnFalse(ServiceClientStatus status)
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct($"Validating {status} -> {status} transition");
        bool result = ServiceClient.ValidateStatusTransition(executionContext, status, status);

        // Assert
        LogAssert("Verifying same-status transition is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateStatusTransition_WithNullFrom_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating null -> Active transition");
        bool result = ServiceClient.ValidateStatusTransition(
            executionContext, null, ServiceClientStatus.Active);

        // Assert
        LogAssert("Verifying null from is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateStatusTransition_WithNullTo_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating Active -> null transition");
        bool result = ServiceClient.ValidateStatusTransition(
            executionContext, ServiceClientStatus.Active, null);

        // Assert
        LogAssert("Verifying null to is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateStatusTransition_ActiveToUndefinedEnumValue_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating execution context");
        var executionContext = CreateTestExecutionContext();

        // Act
        LogAct("Validating Active -> undefined enum value transition");
        bool result = ServiceClient.ValidateStatusTransition(
            executionContext, ServiceClientStatus.Active, (ServiceClientStatus)99);

        // Assert
        LogAssert("Verifying transition is invalid");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region Static IsValid Tests

    [Fact]
    public void IsValid_WithAllValidFields_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating all valid fields");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();
        var createdByUserId = Id.GenerateNewId();

        // Act
        LogAct("Calling IsValid");
        bool result = ServiceClient.IsValid(
            executionContext, entityInfo, "my-client", CreateValidClientSecretHash(),
            "My Service", ServiceClientStatus.Active, createdByUserId);

        // Assert
        LogAssert("Verifying all fields are valid");
        result.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithNullClientId_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null ClientId");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null ClientId");
        bool result = ServiceClient.IsValid(
            executionContext, entityInfo, null, CreateValidClientSecretHash(),
            "My Service", ServiceClientStatus.Active, Id.GenerateNewId());

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullClientSecretHash_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null ClientSecretHash");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null ClientSecretHash");
        bool result = ServiceClient.IsValid(
            executionContext, entityInfo, "my-client", null,
            "My Service", ServiceClientStatus.Active, Id.GenerateNewId());

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullName_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null Name");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null Name");
        bool result = ServiceClient.IsValid(
            executionContext, entityInfo, "my-client", CreateValidClientSecretHash(),
            null, ServiceClientStatus.Active, Id.GenerateNewId());

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullStatus_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null Status");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null Status");
        bool result = ServiceClient.IsValid(
            executionContext, entityInfo, "my-client", CreateValidClientSecretHash(),
            "My Service", null, Id.GenerateNewId());

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_WithNullCreatedByUserId_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating fields with null CreatedByUserId");
        var executionContext = CreateTestExecutionContext();
        var entityInfo = CreateTestEntityInfo();

        // Act
        LogAct("Calling IsValid with null CreatedByUserId");
        bool result = ServiceClient.IsValid(
            executionContext, entityInfo, "my-client", CreateValidClientSecretHash(),
            "My Service", ServiceClientStatus.Active, null);

        // Assert
        LogAssert("Verifying validation fails");
        result.ShouldBeFalse();
    }

    #endregion

    #region Metadata Change Tests

    [Fact]
    public void ChangeClientIdMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original values");
        bool originalIsRequired = ServiceClientMetadata.ClientIdIsRequired;
        int originalMaxLength = ServiceClientMetadata.ClientIdMaxLength;

        try
        {
            // Act
            LogAct("Changing ClientId metadata");
            ServiceClientMetadata.ChangeClientIdMetadata(isRequired: false, maxLength: 512);

            // Assert
            LogAssert("Verifying updated values");
            ServiceClientMetadata.ClientIdIsRequired.ShouldBeFalse();
            ServiceClientMetadata.ClientIdMaxLength.ShouldBe(512);
        }
        finally
        {
            ServiceClientMetadata.ChangeClientIdMetadata(originalIsRequired, originalMaxLength);
        }
    }

    [Fact]
    public void ChangeClientSecretHashMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original values");
        bool originalIsRequired = ServiceClientMetadata.ClientSecretHashIsRequired;
        int originalMaxLength = ServiceClientMetadata.ClientSecretHashMaxLength;

        try
        {
            // Act
            LogAct("Changing ClientSecretHash metadata");
            ServiceClientMetadata.ChangeClientSecretHashMetadata(isRequired: false, maxLength: 2048);

            // Assert
            LogAssert("Verifying updated values");
            ServiceClientMetadata.ClientSecretHashIsRequired.ShouldBeFalse();
            ServiceClientMetadata.ClientSecretHashMaxLength.ShouldBe(2048);
        }
        finally
        {
            ServiceClientMetadata.ChangeClientSecretHashMetadata(originalIsRequired, originalMaxLength);
        }
    }

    [Fact]
    public void ChangeNameMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original values");
        bool originalIsRequired = ServiceClientMetadata.NameIsRequired;
        int originalMaxLength = ServiceClientMetadata.NameMaxLength;

        try
        {
            // Act
            LogAct("Changing Name metadata");
            ServiceClientMetadata.ChangeNameMetadata(isRequired: false, maxLength: 512);

            // Assert
            LogAssert("Verifying updated values");
            ServiceClientMetadata.NameIsRequired.ShouldBeFalse();
            ServiceClientMetadata.NameMaxLength.ShouldBe(512);
        }
        finally
        {
            ServiceClientMetadata.ChangeNameMetadata(originalIsRequired, originalMaxLength);
        }
    }

    [Fact]
    public void ChangeStatusMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = ServiceClientMetadata.StatusIsRequired;

        try
        {
            // Act
            LogAct("Changing Status metadata");
            ServiceClientMetadata.ChangeStatusMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            ServiceClientMetadata.StatusIsRequired.ShouldBeFalse();
        }
        finally
        {
            ServiceClientMetadata.ChangeStatusMetadata(originalIsRequired);
        }
    }

    [Fact]
    public void ChangeCreatedByUserIdMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Saving original value");
        bool originalIsRequired = ServiceClientMetadata.CreatedByUserIdIsRequired;

        try
        {
            // Act
            LogAct("Changing CreatedByUserId metadata");
            ServiceClientMetadata.ChangeCreatedByUserIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying updated value");
            ServiceClientMetadata.CreatedByUserIdIsRequired.ShouldBeFalse();
        }
        finally
        {
            ServiceClientMetadata.ChangeCreatedByUserIdMetadata(originalIsRequired);
        }
    }

    #endregion

    #region Helper Methods

    private static ExecutionContext CreateTestExecutionContext()
    {
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: "test.user",
            executionOrigin: "UnitTest",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: TimeProvider.System);
    }

    private static EntityInfo CreateTestEntityInfo()
    {
        return EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Test Tenant"),
            entityChangeInfo: EntityChangeInfo.CreateFromExistingInfo(
                createdAt: DateTimeOffset.UtcNow,
                createdBy: "creator",
                createdCorrelationId: Guid.NewGuid(),
                createdExecutionOrigin: "UnitTest",
                createdBusinessOperationCode: "TEST_OP",
                lastChangedAt: null,
                lastChangedBy: null,
                lastChangedCorrelationId: null,
                lastChangedExecutionOrigin: null,
                lastChangedBusinessOperationCode: null),
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));
    }

    private static ServiceClient CreateTestServiceClient(ExecutionContext executionContext)
    {
        var input = CreateValidRegisterNewInput();
        return ServiceClient.RegisterNew(executionContext, input)!;
    }

    private static RegisterNewServiceClientInput CreateValidRegisterNewInput()
    {
        var createdByUserId = Id.GenerateNewId();
        var expiresAt = DateTimeOffset.UtcNow.AddDays(365);
        return new RegisterNewServiceClientInput(
            "my-service-client", CreateValidClientSecretHash(),
            "My Service", createdByUserId, expiresAt);
    }

    private static byte[] CreateValidClientSecretHash()
    {
        byte[] bytes = new byte[64]; // typical hash size
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = (byte)((i + 1) % 256);
        }
        return bytes;
    }

    #endregion
}
