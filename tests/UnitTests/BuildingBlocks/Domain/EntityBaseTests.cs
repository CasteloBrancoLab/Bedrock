using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain;
using Bedrock.BuildingBlocks.Domain.Interfaces;
using Bedrock.BuildingBlocks.Domain.Models;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Domain;

public class EntityBaseTests : TestBase
{
    public EntityBaseTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region Test Entity Implementation

    private class TestEntity : EntityBase<TestEntity>
    {
        public string Name { get; private set; } = string.Empty;
        public bool InternalValidationResult { get; set; } = true;

        public TestEntity()
        {
        }

        public TestEntity(EntityInfo entityInfo) : base(entityInfo)
        {
        }

        protected override bool IsValidInternal(ExecutionContext executionContext)
        {
            return InternalValidationResult;
        }

        public override IEntity<TestEntity> Clone()
        {
            return new TestEntity(EntityInfo)
            {
                Name = Name,
                InternalValidationResult = InternalValidationResult
            };
        }

        protected override string CreateMessageCode(string messageSuffix)
        {
            return $"{typeof(TestEntity)}.{messageSuffix}";
        }

        public static TestEntity? CreateNew(
            ExecutionContext executionContext,
            string name)
        {
            return RegisterNewInternal<TestEntity, string>(
                executionContext,
                input: name,
                entityFactory: (ctx, input) => new TestEntity { Name = input },
                handler: (ctx, input, entity) => true);
        }

        public static TestEntity? Modify(
            ExecutionContext executionContext,
            TestEntity instance,
            string newName)
        {
            return RegisterChangeInternal<TestEntity, string>(
                executionContext,
                instance,
                input: newName,
                handler: (ctx, input, entity) =>
                {
                    entity.Name = input;
                    return true;
                });
        }

        public bool CallSetEntityInfo(ExecutionContext executionContext, EntityInfo entityInfo)
        {
            return SetEntityInfo(executionContext, entityInfo);
        }
    }

    private class TestAggregateRoot : EntityBase<TestAggregateRoot>, IAggregateRoot
    {
        public TestAggregateRoot()
        {
        }

        public TestAggregateRoot(EntityInfo entityInfo) : base(entityInfo)
        {
        }

        protected override bool IsValidInternal(ExecutionContext executionContext) => true;

        public override IEntity<TestAggregateRoot> Clone()
        {
            return new TestAggregateRoot(EntityInfo);
        }

        protected override string CreateMessageCode(string messageSuffix)
        {
            return $"{typeof(TestAggregateRoot)}.{messageSuffix}";
        }
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_Default_ShouldCreateWithDefaultEntityInfo()
    {
        // Arrange & Act
        LogAct("Creating entity with default constructor");
        var entity = new TestEntity();

        // Assert
        LogAssert("Verifying EntityInfo is default");
        entity.EntityInfo.Id.Value.ShouldBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithEntityInfo_ShouldSetEntityInfo()
    {
        // Arrange
        LogArrange("Creating EntityInfo");
        var id = Id.CreateFromExistingInfo(Guid.NewGuid());
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Tenant");
        var entityChangeInfo = EntityChangeInfo.CreateFromExistingInfo(
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null);
        var entityVersion = RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow);

        var entityInfo = EntityInfo.CreateFromExistingInfo(id, tenantInfo, entityChangeInfo, entityVersion);

        // Act
        LogAct("Creating entity with EntityInfo");
        var entity = new TestEntity(entityInfo);

        // Assert
        LogAssert("Verifying EntityInfo is set");
        entity.EntityInfo.Id.ShouldBe(id);
        entity.EntityInfo.TenantInfo.ShouldBe(tenantInfo);
    }

    #endregion

    #region IsValid Tests

    [Fact]
    public void IsValid_WithValidEntityInfo_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating valid entity");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider);
        var entity = TestEntity.CreateNew(executionContext, "Test");

        // Act
        LogAct("Calling IsValid");
        var isValid = entity!.IsValid(executionContext);

        // Assert
        LogAssert("Verifying entity is valid");
        isValid.ShouldBeTrue();
    }

    [Fact]
    public void IsValid_WithInvalidInternalValidation_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating entity with failing internal validation");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider);
        var entity = TestEntity.CreateNew(executionContext, "Test");
        entity!.InternalValidationResult = false;

        // Act
        LogAct("Calling IsValid");
        var isValid = entity.IsValid(executionContext);

        // Assert
        LogAssert("Verifying entity is invalid");
        isValid.ShouldBeFalse();
    }

    #endregion

    #region RegisterNewInternal Tests

    [Fact]
    public void RegisterNewInternal_ShouldCreateEntityWithPopulatedEntityInfo()
    {
        // Arrange
        LogArrange("Creating execution context");
        var fixedTime = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);
        var timeProvider = new FixedTimeProvider(fixedTime);
        var correlationId = Guid.NewGuid();
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        var executionContext = ExecutionContext.Create(
            correlationId: correlationId,
            tenantInfo: tenantInfo,
            executionUser: "test.user",
            executionOrigin: "UnitTest",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: timeProvider);

        // Act
        LogAct("Creating new entity via factory");
        var entity = TestEntity.CreateNew(executionContext, "TestName");

        // Assert
        LogAssert("Verifying entity was created");
        entity.ShouldNotBeNull();
        entity.Name.ShouldBe("TestName");

        LogAssert("Verifying EntityInfo is populated");
        entity.EntityInfo.Id.Value.ShouldNotBe(Guid.Empty);
        entity.EntityInfo.TenantInfo.Code.ShouldBe(tenantInfo.Code);
        entity.EntityInfo.EntityChangeInfo.CreatedAt.ShouldBe(fixedTime);
        entity.EntityInfo.EntityChangeInfo.CreatedBy.ShouldBe("test.user");
        entity.EntityInfo.EntityChangeInfo.CreatedCorrelationId.ShouldBe(correlationId);
    }

    [Fact]
    public void RegisterNewInternal_WithFailingHandler_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating execution context");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider);

        // Act - using wrapper method that internally calls RegisterNewInternal with failing handler
        LogAct("Creating entity with failing handler");
        var entity = TestEntityWithFailingHandler.CreateNew(executionContext, "Test");

        // Assert
        LogAssert("Verifying null is returned");
        entity.ShouldBeNull();
    }

    [Fact]
    public void RegisterNewInternal_WithInvalidEntityInfo_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Storing original metadata values");
        var originalCreatedByMinLength = EntityBase.EntityBaseMetadata.CreatedByMinLength;

        try
        {
            // Make CreatedBy validation fail by requiring min length greater than what's provided
            EntityBase.EntityBaseMetadata.ChangeCreationInfoMetadata(
                createdAtIsRequired: true,
                createdByIsRequired: true,
                createdByMinLength: 1000, // Impossibly high min length
                createdByMaxLength: 255);

            var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
            var executionContext = CreateExecutionContext(timeProvider);

            // Act
            LogAct("Creating entity with invalid EntityInfo due to metadata constraints");
            var entity = TestEntity.CreateNew(executionContext, "Test");

            // Assert
            LogAssert("Verifying null is returned when EntityInfo validation fails");
            entity.ShouldBeNull();
        }
        finally
        {
            // Restore original metadata
            EntityBase.EntityBaseMetadata.ChangeCreationInfoMetadata(
                createdAtIsRequired: true,
                createdByIsRequired: true,
                createdByMinLength: originalCreatedByMinLength,
                createdByMaxLength: 255);
        }
    }

    private class TestEntityWithFailingHandler : EntityBase<TestEntityWithFailingHandler>
    {
        public string Name { get; private set; } = string.Empty;

        public TestEntityWithFailingHandler() { }
        public TestEntityWithFailingHandler(EntityInfo entityInfo) : base(entityInfo) { }

        protected override bool IsValidInternal(ExecutionContext executionContext) => true;
        public override IEntity<TestEntityWithFailingHandler> Clone() =>
            new TestEntityWithFailingHandler(EntityInfo) { Name = Name };
        protected override string CreateMessageCode(string messageSuffix) =>
            $"{typeof(TestEntityWithFailingHandler)}.{messageSuffix}";

        public static TestEntityWithFailingHandler? CreateNew(ExecutionContext executionContext, string name)
        {
            return RegisterNewInternal<TestEntityWithFailingHandler, string>(
                executionContext,
                input: name,
                entityFactory: (ctx, input) => new TestEntityWithFailingHandler { Name = input },
                handler: (ctx, input, entity) => false); // Always fails
        }
    }

    private class TestEntityWithExposedMethods : EntityBase<TestEntityWithExposedMethods>
    {
        public string Name { get; private set; } = string.Empty;

        public TestEntityWithExposedMethods() { }
        public TestEntityWithExposedMethods(EntityInfo entityInfo) : base(entityInfo) { }

        protected override bool IsValidInternal(ExecutionContext executionContext) => true;
        public override IEntity<TestEntityWithExposedMethods> Clone() =>
            new TestEntityWithExposedMethods(EntityInfo) { Name = Name };

        // NOTE: No override of CreateMessageCode - this tests the base class implementation

        public static TestEntityWithExposedMethods? CreateNew(ExecutionContext executionContext, string name)
        {
            return RegisterNewInternal<TestEntityWithExposedMethods, string>(
                executionContext,
                input: name,
                entityFactory: (ctx, input) => new TestEntityWithExposedMethods { Name = input },
                handler: (ctx, input, entity) => true);
        }

        public static TestEntityWithExposedMethods? ModifyWithFailingHandler(
            ExecutionContext executionContext,
            TestEntityWithExposedMethods instance,
            string newName)
        {
            return RegisterChangeInternal<TestEntityWithExposedMethods, string>(
                executionContext,
                instance,
                input: newName,
                handler: (ctx, input, entity) =>
                {
                    entity.Name = input;
                    return false; // Handler fails
                });
        }

        // Expose protected static methods for testing
        public static bool TestValidateIfTenantCodeMatchesExecutionContext(
            ExecutionContext executionContext,
            TenantInfo tenantInfo)
        {
            return ValidateIfTenantCodeMatchesExecutionContext(executionContext, tenantInfo);
        }

        public static bool TestValidateTenantForCollection(
            ExecutionContext executionContext,
            IEnumerable<EntityBase> collection)
        {
            return ValidateTenantForCollection(executionContext, collection);
        }

        // Expose the instance CreateMessageCode for testing - calls base class implementation
        public string GetMessageCode(string messageSuffix)
        {
            return CreateMessageCode(messageSuffix);
        }
    }

    #endregion

    #region RegisterChangeInternal Tests

    [Fact]
    public void RegisterChangeInternal_ShouldCloneAndUpdateEntity()
    {
        // Arrange
        LogArrange("Creating original entity");
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider, tenantInfo);
        var original = TestEntity.CreateNew(executionContext, "OriginalName")!;

        var changeTime = new DateTimeOffset(2024, 6, 15, 14, 0, 0, TimeSpan.Zero);
        var changeTimeProvider = new FixedTimeProvider(changeTime);
        var changeContext = CreateExecutionContext(changeTimeProvider, tenantInfo);

        // Act
        LogAct("Modifying entity");
        var modified = TestEntity.Modify(changeContext, original, "NewName");

        // Assert
        LogAssert("Verifying entity was modified");
        modified.ShouldNotBeNull();
        modified.Name.ShouldBe("NewName");
        modified.EntityInfo.Id.ShouldBe(original.EntityInfo.Id);
        modified.EntityInfo.EntityChangeInfo.LastChangedAt.ShouldBe(changeTime);
        modified.EntityInfo.EntityVersion.Value.ShouldNotBe(original.EntityInfo.EntityVersion.Value);

        LogAssert("Verifying original is unchanged");
        original.Name.ShouldBe("OriginalName");
    }

    [Fact]
    public void RegisterChangeInternal_WithTenantMismatch_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating entity with different tenant");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var tenant1 = TenantInfo.Create(Guid.NewGuid(), "Tenant 1");
        var executionContext1 = ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenant1,
            executionUser: "user",
            executionOrigin: "System",
            businessOperationCode: "OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: timeProvider);

        var original = TestEntity.CreateNew(executionContext1, "Test")!;

        var tenant2 = TenantInfo.Create(Guid.NewGuid(), "Tenant 2");
        var executionContext2 = ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenant2,
            executionUser: "user",
            executionOrigin: "System",
            businessOperationCode: "OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: timeProvider);

        // Act
        LogAct("Attempting to modify with different tenant");
        var modified = TestEntity.Modify(executionContext2, original, "NewName");

        // Assert
        LogAssert("Verifying null is returned");
        modified.ShouldBeNull();

        LogAssert("Verifying error message was added");
        executionContext2.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void RegisterChangeInternal_WithFailingHandler_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating entity for modification with failing handler");
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider, tenantInfo);
        var original = TestEntityWithExposedMethods.CreateNew(executionContext, "OriginalName")!;

        // Act
        LogAct("Modifying entity with failing handler");
        var modified = TestEntityWithExposedMethods.ModifyWithFailingHandler(executionContext, original, "NewName");

        // Assert
        LogAssert("Verifying null is returned when handler fails");
        modified.ShouldBeNull();
    }

    [Fact]
    public void RegisterChangeInternal_WithInvalidEntityInfo_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Creating entity and storing original metadata values");
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider, tenantInfo);
        var original = TestEntity.CreateNew(executionContext, "OriginalName")!;

        var originalLastChangedByMinLength = EntityBase.EntityBaseMetadata.LastChangedByMinLength;

        try
        {
            // Make LastChangedBy validation fail by requiring min length greater than what's provided
            EntityBase.EntityBaseMetadata.ChangeUpdateInfoMetadata(
                lastChangedAtIsRequired: false,
                lastChangedByIsRequired: true,
                lastChangedByMinLength: 1000, // Impossibly high min length
                lastChangedByMaxLength: 255);

            // Act
            LogAct("Modifying entity with invalid EntityInfo due to metadata constraints");
            var modified = TestEntity.Modify(executionContext, original, "NewName");

            // Assert
            LogAssert("Verifying null is returned when EntityInfo validation fails");
            modified.ShouldBeNull();
        }
        finally
        {
            // Restore original metadata
            EntityBase.EntityBaseMetadata.ChangeUpdateInfoMetadata(
                lastChangedAtIsRequired: false,
                lastChangedByIsRequired: false,
                lastChangedByMinLength: originalLastChangedByMinLength,
                lastChangedByMaxLength: 255);
        }
    }

    #endregion

    #region SetEntityInfo Tests

    [Fact]
    public void SetEntityInfo_WithValidInfo_ShouldSetAndReturnTrue()
    {
        // Arrange
        LogArrange("Creating entity and valid EntityInfo");
        var entity = new TestEntity();
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider);
        var entityInfo = CreateValidEntityInfo();

        // Act
        LogAct("Calling SetEntityInfo");
        var result = entity.CallSetEntityInfo(executionContext, entityInfo);

        // Assert
        LogAssert("Verifying EntityInfo was set");
        result.ShouldBeTrue();
        entity.EntityInfo.ShouldBe(entityInfo);
    }

    [Fact]
    public void SetEntityInfo_WithInvalidInfo_ShouldReturnFalseAndNotSetEntityInfo()
    {
        // Arrange
        LogArrange("Creating entity and invalid EntityInfo with empty Id");
        var entity = new TestEntity();
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider);
        var invalidEntityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.Empty), // Invalid - empty Id
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Tenant"),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        var originalEntityInfo = entity.EntityInfo;

        // Act
        LogAct("Calling SetEntityInfo with invalid info");
        var result = entity.CallSetEntityInfo(executionContext, invalidEntityInfo);

        // Assert
        LogAssert("Verifying EntityInfo was NOT set and false was returned");
        result.ShouldBeFalse();
        entity.EntityInfo.ShouldBe(originalEntityInfo);
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateEntityInfo Tests

    [Fact]
    public void ValidateEntityInfo_WithValidInfo_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating valid EntityInfo");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider);
        var entityInfo = CreateValidEntityInfo();

        // Act
        LogAct("Validating EntityInfo");
        var isValid = EntityBase.ValidateEntityInfo(executionContext, entityInfo);

        // Assert
        LogAssert("Verifying validation passed");
        isValid.ShouldBeTrue();
        executionContext.HasErrorMessages.ShouldBeFalse();
    }

    [Fact]
    public void ValidateEntityInfo_WithEmptyId_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating EntityInfo with empty Id");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider);
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.Empty),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Tenant"),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        // Act
        LogAct("Validating EntityInfo");
        var isValid = EntityBase.ValidateEntityInfo(executionContext, entityInfo);

        // Assert
        LogAssert("Verifying validation failed");
        isValid.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateEntityInfo_WithEmptyTenantCode_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating EntityInfo with empty tenant code");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider);
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.Empty, "Tenant"),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        // Act
        LogAct("Validating EntityInfo");
        var isValid = EntityBase.ValidateEntityInfo(executionContext, entityInfo);

        // Assert
        LogAssert("Verifying validation failed");
        isValid.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateEntityInfo_WithEmptyCreatedBy_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating EntityInfo with empty CreatedBy");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider);
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Tenant"),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: string.Empty,
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        // Act
        LogAct("Validating EntityInfo");
        var isValid = EntityBase.ValidateEntityInfo(executionContext, entityInfo);

        // Assert
        LogAssert("Verifying validation failed");
        isValid.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateEntityInfo_WithCreatedByExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating EntityInfo with CreatedBy exceeding max length");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider);
        var longCreatedBy = new string('a', 256);
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Tenant"),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: longCreatedBy,
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        // Act
        LogAct("Validating EntityInfo");
        var isValid = EntityBase.ValidateEntityInfo(executionContext, entityInfo);

        // Assert
        LogAssert("Verifying validation failed");
        isValid.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateEntityInfo_WithLastChangedByExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating EntityInfo with LastChangedBy exceeding max length");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider);
        var longLastChangedBy = new string('a', 256);
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Tenant"),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: DateTimeOffset.UtcNow,
            lastChangedBy: longLastChangedBy,
            lastChangedCorrelationId: Guid.NewGuid(),
            lastChangedExecutionOrigin: "System",
            lastChangedBusinessOperationCode: "OP",
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        // Act
        LogAct("Validating EntityInfo");
        var isValid = EntityBase.ValidateEntityInfo(executionContext, entityInfo);

        // Assert
        LogAssert("Verifying validation failed");
        isValid.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateEntityInfo_WithEmptyCreatedExecutionOrigin_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating EntityInfo with empty CreatedExecutionOrigin");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider);
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Tenant"),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: string.Empty,
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        // Act
        LogAct("Validating EntityInfo");
        var isValid = EntityBase.ValidateEntityInfo(executionContext, entityInfo);

        // Assert
        LogAssert("Verifying validation failed");
        isValid.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateEntityInfo_WithCreatedExecutionOriginExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating EntityInfo with CreatedExecutionOrigin exceeding max length");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider);
        var longOrigin = new string('a', 256);
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Tenant"),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: longOrigin,
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        // Act
        LogAct("Validating EntityInfo");
        var isValid = EntityBase.ValidateEntityInfo(executionContext, entityInfo);

        // Assert
        LogAssert("Verifying validation failed");
        isValid.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateEntityInfo_WithLastChangedExecutionOriginExceedingMaxLength_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating EntityInfo with LastChangedExecutionOrigin exceeding max length");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider);
        var longOrigin = new string('a', 256);
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Tenant"),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: DateTimeOffset.UtcNow,
            lastChangedBy: "modifier",
            lastChangedCorrelationId: Guid.NewGuid(),
            lastChangedExecutionOrigin: longOrigin,
            lastChangedBusinessOperationCode: "OP",
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        // Act
        LogAct("Validating EntityInfo");
        var isValid = EntityBase.ValidateEntityInfo(executionContext, entityInfo);

        // Assert
        LogAssert("Verifying validation failed");
        isValid.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateEntityInfo_WithEmptyEntityVersion_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating EntityInfo with empty EntityVersion");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider);
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Tenant"),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(0L));

        // Act
        LogAct("Validating EntityInfo");
        var isValid = EntityBase.ValidateEntityInfo(executionContext, entityInfo);

        // Assert
        LogAssert("Verifying validation failed");
        isValid.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    [Fact]
    public void ValidateEntityInfo_WithEmptyCreatedCorrelationId_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating EntityInfo with empty CreatedCorrelationId");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider);
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Tenant"),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "creator",
            createdCorrelationId: Guid.Empty,
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        // Act
        LogAct("Validating EntityInfo");
        var isValid = EntityBase.ValidateEntityInfo(executionContext, entityInfo);

        // Assert
        LogAssert("Verifying validation failed");
        isValid.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
    }

    #endregion

    #region ValidateIfTenantCodeMatchesExecutionContext Tests

    [Fact]
    public void ValidateIfTenantCodeMatchesExecutionContext_Matching_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating context and tenantInfo with matching tenant codes");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        var executionContext = ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: "user",
            executionOrigin: "System",
            businessOperationCode: "OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: timeProvider);

        // Act - directly call the protected method through exposed test method
        LogAct("Validating tenant match directly");
        var result = TestEntityWithExposedMethods.TestValidateIfTenantCodeMatchesExecutionContext(
            executionContext, tenantInfo);

        // Assert
        LogAssert("Verifying validation passed");
        result.ShouldBeTrue();
        executionContext.HasErrorMessages.ShouldBeFalse();
    }

    [Fact]
    public void ValidateIfTenantCodeMatchesExecutionContext_Mismatching_ShouldReturnFalseAndAddError()
    {
        // Arrange
        LogArrange("Creating execution context with different tenant than provided tenantInfo");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var contextTenantInfo = TenantInfo.Create(Guid.NewGuid(), "Context Tenant");
        var differentTenantInfo = TenantInfo.Create(Guid.NewGuid(), "Different Tenant");
        var executionContext = ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: contextTenantInfo,
            executionUser: "user",
            executionOrigin: "System",
            businessOperationCode: "OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: timeProvider);

        // Act
        LogAct("Validating tenant mismatch");
        var result = TestEntityWithExposedMethods.TestValidateIfTenantCodeMatchesExecutionContext(
            executionContext, differentTenantInfo);

        // Assert
        LogAssert("Verifying validation failed and error was added");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
        var messages = executionContext.Messages.ToList();
        messages.ShouldContain(m => m.Code.Contains("TenantMismatch"));
    }

    #endregion

    #region ValidateTenantForCollection Tests

    [Fact]
    public void ValidateTenantForCollection_AllMatching_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating entities with matching tenant");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var tenantInfo = TenantInfo.Create(Guid.NewGuid(), "Test Tenant");
        var executionContext = ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: "user",
            executionOrigin: "System",
            businessOperationCode: "OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: timeProvider);

        var entities = new[]
        {
            TestEntityWithExposedMethods.CreateNew(executionContext, "Test1")!,
            TestEntityWithExposedMethods.CreateNew(executionContext, "Test2")!,
            TestEntityWithExposedMethods.CreateNew(executionContext, "Test3")!
        };

        // Act
        LogAct("Validating tenant for collection");
        var result = TestEntityWithExposedMethods.TestValidateTenantForCollection(executionContext, entities);

        // Assert
        LogAssert("Verifying validation passed");
        result.ShouldBeTrue();
    }

    [Fact]
    public void ValidateTenantForCollection_OneMismatching_ShouldReturnFalseAndAddError()
    {
        // Arrange
        LogArrange("Creating entities with one having different tenant");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var tenant1 = TenantInfo.Create(Guid.NewGuid(), "Tenant 1");
        var tenant2 = TenantInfo.Create(Guid.NewGuid(), "Tenant 2");

        var executionContext1 = ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenant1,
            executionUser: "user",
            executionOrigin: "System",
            businessOperationCode: "OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: timeProvider);

        var executionContext2 = ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenant2,
            executionUser: "user",
            executionOrigin: "System",
            businessOperationCode: "OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: timeProvider);

        var entities = new EntityBase[]
        {
            TestEntityWithExposedMethods.CreateNew(executionContext1, "Test1")!,
            TestEntityWithExposedMethods.CreateNew(executionContext2, "Test2")!, // Different tenant
            TestEntityWithExposedMethods.CreateNew(executionContext1, "Test3")!
        };

        // Act
        LogAct("Validating tenant for collection with mismatching entity");
        var result = TestEntityWithExposedMethods.TestValidateTenantForCollection(executionContext1, entities);

        // Assert
        LogAssert("Verifying validation failed and error was added");
        result.ShouldBeFalse();
        executionContext1.HasErrorMessages.ShouldBeTrue();
        var messages = executionContext1.Messages.ToList();
        messages.ShouldContain(m => m.Code.Contains("TenantMismatch"));
    }

    #endregion

    #region CreateMessageCode Tests

    [Fact]
    public void CreateMessageCode_ShouldIncludeEntityTypeName()
    {
        // Arrange
        LogArrange("Creating entity");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider);

        // Create entity with empty CreatedBy to trigger validation error
        var entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Tenant"),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: string.Empty,
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        // Act
        LogAct("Validating to generate message codes");
        EntityBase.ValidateEntityInfo(executionContext, entityInfo);

        // Assert
        LogAssert("Verifying message codes contain EntityBase");
        executionContext.HasErrorMessages.ShouldBeTrue();
        var messages = executionContext.Messages.ToList();
        messages.ShouldContain(m => m.Code.Contains("EntityBase"));
    }

    [Fact]
    public void CreateMessageCode_Generic_ShouldIncludeEntityTypeName()
    {
        // Arrange
        LogArrange("Creating entity with exposed CreateMessageCode");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider);
        var entity = TestEntityWithExposedMethods.CreateNew(executionContext, "Test")!;

        // Act
        LogAct("Calling CreateMessageCode on generic entity");
        var messageCode = entity.GetMessageCode("TestSuffix");

        // Assert
        LogAssert("Verifying message code contains entity type name and suffix");
        messageCode.ShouldContain("TestEntityWithExposedMethods");
        messageCode.ShouldContain("TestSuffix");
        messageCode.ShouldBe($"{typeof(TestEntityWithExposedMethods)}.TestSuffix");
    }

    #endregion

    #region EntityBaseMetadata Tests

    #region PropertyName Tests

    [Fact]
    public void EntityBaseMetadata_IdPropertyName_ShouldReturnCorrectValue()
    {
        // Act & Assert
        LogAct("Checking IdPropertyName");
        EntityBase.EntityBaseMetadata.IdPropertyName.ShouldBe("Id");
    }

    [Fact]
    public void EntityBaseMetadata_TenantCodePropertyName_ShouldReturnCorrectValue()
    {
        // Act & Assert
        LogAct("Checking TenantCodePropertyName");
        EntityBase.EntityBaseMetadata.TenantCodePropertyName.ShouldContain("EntityInfo");
        EntityBase.EntityBaseMetadata.TenantCodePropertyName.ShouldContain("EntityChangeInfo");
        EntityBase.EntityBaseMetadata.TenantCodePropertyName.ShouldContain("Code");
    }

    [Fact]
    public void EntityBaseMetadata_CreatedAtPropertyName_ShouldReturnCorrectValue()
    {
        // Act & Assert
        LogAct("Checking CreatedAtPropertyName");
        EntityBase.EntityBaseMetadata.CreatedAtPropertyName.ShouldContain("EntityInfo");
        EntityBase.EntityBaseMetadata.CreatedAtPropertyName.ShouldContain("EntityChangeInfo");
        EntityBase.EntityBaseMetadata.CreatedAtPropertyName.ShouldContain("CreatedAt");
    }

    [Fact]
    public void EntityBaseMetadata_CreatedByPropertyName_ShouldReturnCorrectValue()
    {
        // Act & Assert
        LogAct("Checking CreatedByPropertyName");
        EntityBase.EntityBaseMetadata.CreatedByPropertyName.ShouldContain("EntityInfo");
        EntityBase.EntityBaseMetadata.CreatedByPropertyName.ShouldContain("EntityChangeInfo");
        EntityBase.EntityBaseMetadata.CreatedByPropertyName.ShouldContain("CreatedBy");
    }

    [Fact]
    public void EntityBaseMetadata_LastChangedAtPropertyName_ShouldReturnCorrectValue()
    {
        // Act & Assert
        LogAct("Checking LastChangedAtPropertyName");
        EntityBase.EntityBaseMetadata.LastChangedAtPropertyName.ShouldContain("EntityInfo");
        EntityBase.EntityBaseMetadata.LastChangedAtPropertyName.ShouldContain("EntityChangeInfo");
        EntityBase.EntityBaseMetadata.LastChangedAtPropertyName.ShouldContain("LastChangedAt");
    }

    [Fact]
    public void EntityBaseMetadata_LastChangedByPropertyName_ShouldReturnCorrectValue()
    {
        // Act & Assert
        LogAct("Checking LastChangedByPropertyName");
        EntityBase.EntityBaseMetadata.LastChangedByPropertyName.ShouldContain("EntityInfo");
        EntityBase.EntityBaseMetadata.LastChangedByPropertyName.ShouldContain("EntityChangeInfo");
        EntityBase.EntityBaseMetadata.LastChangedByPropertyName.ShouldContain("LastChangedBy");
    }

    [Fact]
    public void EntityBaseMetadata_CreatedCorrelationIdPropertyName_ShouldReturnCorrectValue()
    {
        // Act & Assert
        LogAct("Checking CreatedCorrelationIdPropertyName");
        EntityBase.EntityBaseMetadata.CreatedCorrelationIdPropertyName.ShouldContain("EntityInfo");
        EntityBase.EntityBaseMetadata.CreatedCorrelationIdPropertyName.ShouldContain("EntityChangeInfo");
        EntityBase.EntityBaseMetadata.CreatedCorrelationIdPropertyName.ShouldContain("CreatedCorrelationId");
    }

    [Fact]
    public void EntityBaseMetadata_LastChangedCorrelationIdPropertyName_ShouldReturnCorrectValue()
    {
        // Act & Assert
        LogAct("Checking LastChangedCorrelationIdPropertyName");
        EntityBase.EntityBaseMetadata.LastChangedCorrelationIdPropertyName.ShouldContain("EntityInfo");
        EntityBase.EntityBaseMetadata.LastChangedCorrelationIdPropertyName.ShouldContain("EntityChangeInfo");
        EntityBase.EntityBaseMetadata.LastChangedCorrelationIdPropertyName.ShouldContain("LastChangedCorrelationId");
    }

    [Fact]
    public void EntityBaseMetadata_CreatedExecutionOriginPropertyName_ShouldReturnCorrectValue()
    {
        // Act & Assert
        LogAct("Checking CreatedExecutionOriginPropertyName");
        EntityBase.EntityBaseMetadata.CreatedExecutionOriginPropertyName.ShouldContain("EntityInfo");
        EntityBase.EntityBaseMetadata.CreatedExecutionOriginPropertyName.ShouldContain("EntityChangeInfo");
        EntityBase.EntityBaseMetadata.CreatedExecutionOriginPropertyName.ShouldContain("CreatedExecutionOrigin");
    }

    [Fact]
    public void EntityBaseMetadata_LastChangedExecutionOriginPropertyName_ShouldReturnCorrectValue()
    {
        // Act & Assert
        LogAct("Checking LastChangedExecutionOriginPropertyName");
        EntityBase.EntityBaseMetadata.LastChangedExecutionOriginPropertyName.ShouldContain("EntityInfo");
        EntityBase.EntityBaseMetadata.LastChangedExecutionOriginPropertyName.ShouldContain("EntityChangeInfo");
        EntityBase.EntityBaseMetadata.LastChangedExecutionOriginPropertyName.ShouldContain("LastChangedExecutionOrigin");
    }

    [Fact]
    public void EntityBaseMetadata_EntityVersionPropertyName_ShouldReturnCorrectValue()
    {
        // Act & Assert
        LogAct("Checking EntityVersionPropertyName");
        EntityBase.EntityBaseMetadata.EntityVersionPropertyName.ShouldContain("EntityInfo");
        EntityBase.EntityBaseMetadata.EntityVersionPropertyName.ShouldContain("EntityVersion");
    }

    #endregion

    #region Default Values Tests

    [Fact]
    public void EntityBaseMetadata_DefaultValues_IdIsRequired_ShouldBeTrue()
    {
        // Act & Assert
        LogAct("Checking default IdIsRequired");
        EntityBase.EntityBaseMetadata.IdIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void EntityBaseMetadata_DefaultValues_TenantCodeIsRequired_ShouldBeTrue()
    {
        // Act & Assert
        LogAct("Checking default TenantCodeIsRequired");
        EntityBase.EntityBaseMetadata.TenantCodeIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void EntityBaseMetadata_DefaultValues_CreatedAtIsRequired_ShouldBeTrue()
    {
        // Act & Assert
        LogAct("Checking default CreatedAtIsRequired");
        EntityBase.EntityBaseMetadata.CreatedAtIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void EntityBaseMetadata_DefaultValues_CreatedByIsRequired_ShouldBeTrue()
    {
        // Act & Assert
        LogAct("Checking default CreatedByIsRequired");
        EntityBase.EntityBaseMetadata.CreatedByIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void EntityBaseMetadata_DefaultValues_CreatedByIsRequired_ShouldFailValidationWhenNull()
    {
        // Arrange
        LogArrange("Creating EntityInfo with null CreatedBy to verify default behavior");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider);
        var entityInfoWithNullCreatedBy = EntityInfo.CreateFromExistingInfo(
            id: Id.GenerateNewId(),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Tenant"),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: null, // This should fail because CreatedByIsRequired defaults to true
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));

        // Act
        LogAct("Validating EntityInfo with null CreatedBy");
        var result = EntityBase.ValidateEntityInfo(executionContext, entityInfoWithNullCreatedBy);

        // Assert
        LogAssert("Verifying validation fails because CreatedByIsRequired is true by default");
        result.ShouldBeFalse();
        executionContext.HasErrorMessages.ShouldBeTrue();
        var messages = executionContext.Messages.ToList();
        messages.ShouldContain(m => m.Code.Contains("CreatedBy"));
    }

    [Fact]
    public void EntityBaseMetadata_DefaultValues_CreatedCorrelationIdIsRequired_ShouldBeTrue()
    {
        // Act & Assert
        LogAct("Checking default CreatedCorrelationIdIsRequired");
        EntityBase.EntityBaseMetadata.CreatedCorrelationIdIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void EntityBaseMetadata_DefaultValues_CreatedExecutionOriginIsRequired_ShouldBeTrue()
    {
        // Act & Assert
        LogAct("Checking default CreatedExecutionOriginIsRequired");
        EntityBase.EntityBaseMetadata.CreatedExecutionOriginIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void EntityBaseMetadata_DefaultValues_EntityVersionIsRequired_ShouldBeTrue()
    {
        // Act & Assert
        LogAct("Checking default EntityVersionIsRequired");
        EntityBase.EntityBaseMetadata.EntityVersionIsRequired.ShouldBeTrue();
    }

    [Fact]
    public void EntityBaseMetadata_DefaultValues_LastChangedAtIsRequired_ShouldBeFalse()
    {
        // Act & Assert
        LogAct("Checking default LastChangedAtIsRequired");
        EntityBase.EntityBaseMetadata.LastChangedAtIsRequired.ShouldBeFalse();
    }

    [Fact]
    public void EntityBaseMetadata_DefaultValues_LastChangedByIsRequired_ShouldBeFalse()
    {
        // Act & Assert
        LogAct("Checking default LastChangedByIsRequired");
        EntityBase.EntityBaseMetadata.LastChangedByIsRequired.ShouldBeFalse();
    }

    [Fact]
    public void EntityBaseMetadata_DefaultValues_LastChangedCorrelationIdIsRequired_ShouldBeFalse()
    {
        // Act & Assert
        LogAct("Checking default LastChangedCorrelationIdIsRequired");
        EntityBase.EntityBaseMetadata.LastChangedCorrelationIdIsRequired.ShouldBeFalse();
    }

    [Fact]
    public void EntityBaseMetadata_DefaultValues_LastChangedExecutionOriginIsRequired_ShouldBeFalse()
    {
        // Act & Assert
        LogAct("Checking default LastChangedExecutionOriginIsRequired");
        EntityBase.EntityBaseMetadata.LastChangedExecutionOriginIsRequired.ShouldBeFalse();
    }

    #endregion

    #region Change Methods Tests

    [Fact]
    public void EntityBaseMetadata_ChangeIdMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Storing original value");
        var originalValue = EntityBase.EntityBaseMetadata.IdIsRequired;

        try
        {
            // Act
            LogAct("Changing Id metadata");
            EntityBase.EntityBaseMetadata.ChangeIdMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying value changed");
            EntityBase.EntityBaseMetadata.IdIsRequired.ShouldBeFalse();
        }
        finally
        {
            // Restore
            EntityBase.EntityBaseMetadata.ChangeIdMetadata(isRequired: originalValue);
        }
    }

    [Fact]
    public void EntityBaseMetadata_ChangeTenantCodeMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Storing original value");
        var originalValue = EntityBase.EntityBaseMetadata.TenantCodeIsRequired;

        try
        {
            // Act
            LogAct("Changing TenantCode metadata");
            EntityBase.EntityBaseMetadata.ChangeTenantCodeMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying value changed");
            EntityBase.EntityBaseMetadata.TenantCodeIsRequired.ShouldBeFalse();
        }
        finally
        {
            // Restore
            EntityBase.EntityBaseMetadata.ChangeTenantCodeMetadata(isRequired: originalValue);
        }
    }

    [Fact]
    public void EntityBaseMetadata_ChangeCreationInfoMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Storing original values");
        var originalCreatedAtIsRequired = EntityBase.EntityBaseMetadata.CreatedAtIsRequired;
        var originalCreatedByIsRequired = EntityBase.EntityBaseMetadata.CreatedByIsRequired;
        var originalCreatedByMinLength = EntityBase.EntityBaseMetadata.CreatedByMinLength;
        var originalCreatedByMaxLength = EntityBase.EntityBaseMetadata.CreatedByMaxLength;

        try
        {
            // Act
            LogAct("Changing creation info metadata");
            EntityBase.EntityBaseMetadata.ChangeCreationInfoMetadata(
                createdAtIsRequired: false,
                createdByIsRequired: false,
                createdByMinLength: 5,
                createdByMaxLength: 100);

            // Assert
            LogAssert("Verifying values changed");
            EntityBase.EntityBaseMetadata.CreatedAtIsRequired.ShouldBeFalse();
            EntityBase.EntityBaseMetadata.CreatedByIsRequired.ShouldBeFalse();
            EntityBase.EntityBaseMetadata.CreatedByMinLength.ShouldBe(5);
            EntityBase.EntityBaseMetadata.CreatedByMaxLength.ShouldBe(100);
        }
        finally
        {
            // Restore
            EntityBase.EntityBaseMetadata.ChangeCreationInfoMetadata(
                originalCreatedAtIsRequired,
                originalCreatedByIsRequired,
                originalCreatedByMinLength,
                originalCreatedByMaxLength);
        }
    }

    [Fact]
    public void EntityBaseMetadata_ChangeUpdateInfoMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Storing original values");
        var originalLastChangedAtIsRequired = EntityBase.EntityBaseMetadata.LastChangedAtIsRequired;
        var originalLastChangedByIsRequired = EntityBase.EntityBaseMetadata.LastChangedByIsRequired;
        var originalLastChangedByMinLength = EntityBase.EntityBaseMetadata.LastChangedByMinLength;
        var originalLastChangedByMaxLength = EntityBase.EntityBaseMetadata.LastChangedByMaxLength;

        try
        {
            // Act
            LogAct("Changing update info metadata");
            EntityBase.EntityBaseMetadata.ChangeUpdateInfoMetadata(
                lastChangedAtIsRequired: true,
                lastChangedByIsRequired: true,
                lastChangedByMinLength: 5,
                lastChangedByMaxLength: 100);

            // Assert
            LogAssert("Verifying values changed");
            EntityBase.EntityBaseMetadata.LastChangedAtIsRequired.ShouldBeTrue();
            EntityBase.EntityBaseMetadata.LastChangedByIsRequired.ShouldBeTrue();
            EntityBase.EntityBaseMetadata.LastChangedByMinLength.ShouldBe(5);
            EntityBase.EntityBaseMetadata.LastChangedByMaxLength.ShouldBe(100);
        }
        finally
        {
            // Restore
            EntityBase.EntityBaseMetadata.ChangeUpdateInfoMetadata(
                originalLastChangedAtIsRequired,
                originalLastChangedByIsRequired,
                originalLastChangedByMinLength,
                originalLastChangedByMaxLength);
        }
    }

    [Fact]
    public void EntityBaseMetadata_ChangeEntityVersionMetadata_ShouldUpdateIsRequired()
    {
        // Arrange
        LogArrange("Storing original value");
        var originalValue = EntityBase.EntityBaseMetadata.EntityVersionIsRequired;

        try
        {
            // Act
            LogAct("Changing EntityVersion metadata");
            EntityBase.EntityBaseMetadata.ChangeEntityVersionMetadata(isRequired: false);

            // Assert
            LogAssert("Verifying value changed");
            EntityBase.EntityBaseMetadata.EntityVersionIsRequired.ShouldBeFalse();
        }
        finally
        {
            // Restore
            EntityBase.EntityBaseMetadata.ChangeEntityVersionMetadata(isRequired: originalValue);
        }
    }

    [Fact]
    public void EntityBaseMetadata_ChangeCorrelationIdMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Storing original values");
        var originalCreatedCorrelationIdIsRequired = EntityBase.EntityBaseMetadata.CreatedCorrelationIdIsRequired;
        var originalLastChangedCorrelationIdIsRequired = EntityBase.EntityBaseMetadata.LastChangedCorrelationIdIsRequired;

        try
        {
            // Act
            LogAct("Changing correlation ID metadata");
            EntityBase.EntityBaseMetadata.ChangeCorrelationIdMetadata(
                createdCorrelationIdIsRequired: false,
                lastChangedCorrelationIdIsRequired: true);

            // Assert
            LogAssert("Verifying values changed");
            EntityBase.EntityBaseMetadata.CreatedCorrelationIdIsRequired.ShouldBeFalse();
            EntityBase.EntityBaseMetadata.LastChangedCorrelationIdIsRequired.ShouldBeTrue();
        }
        finally
        {
            // Restore
            EntityBase.EntityBaseMetadata.ChangeCorrelationIdMetadata(
                originalCreatedCorrelationIdIsRequired,
                originalLastChangedCorrelationIdIsRequired);
        }
    }

    [Fact]
    public void EntityBaseMetadata_ChangeExecutionOriginMetadata_ShouldUpdateValues()
    {
        // Arrange
        LogArrange("Storing original values");
        var originalCreatedExecutionOriginIsRequired = EntityBase.EntityBaseMetadata.CreatedExecutionOriginIsRequired;
        var originalCreatedExecutionOriginMinLength = EntityBase.EntityBaseMetadata.CreatedExecutionOriginMinLength;
        var originalCreatedExecutionOriginMaxLength = EntityBase.EntityBaseMetadata.CreatedExecutionOriginMaxLength;
        var originalLastChangedExecutionOriginIsRequired = EntityBase.EntityBaseMetadata.LastChangedExecutionOriginIsRequired;
        var originalLastChangedExecutionOriginMinLength = EntityBase.EntityBaseMetadata.LastChangedExecutionOriginMinLength;
        var originalLastChangedExecutionOriginMaxLength = EntityBase.EntityBaseMetadata.LastChangedExecutionOriginMaxLength;

        try
        {
            // Act
            LogAct("Changing execution origin metadata");
            EntityBase.EntityBaseMetadata.ChangeExecutionOriginMetadata(
                createdExecutionOriginIsRequired: false,
                createdExecutionOriginMinLength: 5,
                createdExecutionOriginMaxLength: 100,
                lastChangedExecutionOriginIsRequired: true,
                lastChangedExecutionOriginMinLength: 10,
                lastChangedExecutionOriginMaxLength: 200);

            // Assert
            LogAssert("Verifying values changed");
            EntityBase.EntityBaseMetadata.CreatedExecutionOriginIsRequired.ShouldBeFalse();
            EntityBase.EntityBaseMetadata.CreatedExecutionOriginMinLength.ShouldBe(5);
            EntityBase.EntityBaseMetadata.CreatedExecutionOriginMaxLength.ShouldBe(100);
            EntityBase.EntityBaseMetadata.LastChangedExecutionOriginIsRequired.ShouldBeTrue();
            EntityBase.EntityBaseMetadata.LastChangedExecutionOriginMinLength.ShouldBe(10);
            EntityBase.EntityBaseMetadata.LastChangedExecutionOriginMaxLength.ShouldBe(200);
        }
        finally
        {
            // Restore
            EntityBase.EntityBaseMetadata.ChangeExecutionOriginMetadata(
                originalCreatedExecutionOriginIsRequired,
                originalCreatedExecutionOriginMinLength,
                originalCreatedExecutionOriginMaxLength,
                originalLastChangedExecutionOriginIsRequired,
                originalLastChangedExecutionOriginMinLength,
                originalLastChangedExecutionOriginMaxLength);
        }
    }

    #endregion

    #endregion

    #region TenantMismatchMessageCode Tests

    [Fact]
    public void TenantMismatchMessageCode_ShouldBeCorrectValue()
    {
        // Arrange & Act
        LogAct("Reading TenantMismatchMessageCode constant");
        var code = EntityBase.TenantMismatchMessageCode;

        // Assert
        LogAssert("Verifying constant value");
        code.ShouldBe("TenantMismatch");
    }

    #endregion

    #region IAggregateRoot Tests

    [Fact]
    public void IAggregateRoot_ShouldBeImplementableAsMarkerInterface()
    {
        // Arrange & Act
        LogAct("Creating aggregate root instance");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var entityInfo = CreateValidEntityInfo();
        var aggregateRoot = new TestAggregateRoot(entityInfo);

        // Assert
        LogAssert("Verifying aggregate root implements IAggregateRoot");
        aggregateRoot.ShouldBeAssignableTo<IAggregateRoot>();
        aggregateRoot.ShouldBeAssignableTo<IEntity>();
    }

    #endregion

    #region Clone Tests

    [Fact]
    public void Clone_ShouldCreateDeepCopy()
    {
        // Arrange
        LogArrange("Creating entity to clone");
        var timeProvider = new FixedTimeProvider(DateTimeOffset.UtcNow);
        var executionContext = CreateExecutionContext(timeProvider);
        var original = TestEntity.CreateNew(executionContext, "OriginalName")!;

        // Act
        LogAct("Cloning entity");
        var clone = (TestEntity)original.Clone();

        // Assert
        LogAssert("Verifying clone is separate instance");
        clone.ShouldNotBeSameAs(original);
        clone.Name.ShouldBe(original.Name);
        clone.EntityInfo.Id.ShouldBe(original.EntityInfo.Id);
    }

    #endregion

    #region Helper Methods

    private static ExecutionContext CreateExecutionContext(TimeProvider timeProvider)
    {
        return CreateExecutionContext(timeProvider, TenantInfo.Create(Guid.NewGuid(), "Test Tenant"));
    }

    private static ExecutionContext CreateExecutionContext(TimeProvider timeProvider, TenantInfo tenantInfo)
    {
        return ExecutionContext.Create(
            correlationId: Guid.NewGuid(),
            tenantInfo: tenantInfo,
            executionUser: "test.user",
            executionOrigin: "UnitTest",
            businessOperationCode: "TEST_OP",
            minimumMessageType: MessageType.Trace,
            timeProvider: timeProvider);
    }

    private static EntityInfo CreateValidEntityInfo()
    {
        return EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(Guid.NewGuid()),
            tenantInfo: TenantInfo.Create(Guid.NewGuid(), "Test Tenant"),
            createdAt: DateTimeOffset.UtcNow,
            createdBy: "creator",
            createdCorrelationId: Guid.NewGuid(),
            createdExecutionOrigin: "System",
            createdBusinessOperationCode: "OP",
            lastChangedAt: null,
            lastChangedBy: null,
            lastChangedCorrelationId: null,
            lastChangedExecutionOrigin: null,
            lastChangedBusinessOperationCode: null,
            entityVersion: RegistryVersion.CreateFromExistingInfo(DateTimeOffset.UtcNow));
    }

    private class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _fixedTime;

        public FixedTimeProvider(DateTimeOffset fixedTime)
        {
            _fixedTime = fixedTime;
        }

        public override DateTimeOffset GetUtcNow() => _fixedTime;
    }

    #endregion
}
