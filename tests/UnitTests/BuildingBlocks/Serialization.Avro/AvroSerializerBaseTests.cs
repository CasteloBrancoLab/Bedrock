using System.Text;
using Bedrock.BuildingBlocks.Serialization.Avro;
using Bedrock.BuildingBlocks.Serialization.Avro.Models;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Serialization.Avro;

public class AvroSerializerBaseTests : TestBase
{
    private readonly TestAvroSerializer _serializer;

    public AvroSerializerBaseTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _serializer = new TestAvroSerializer();
    }

    #region Serialize Tests

    [Fact]
    public void Serialize_WithValidObject_ShouldReturnBytes()
    {
        // Arrange
        LogArrange("Creating test object");
        var testObject = new TestDto { Name = "Test", Value = 42 };

        // Act
        LogAct("Serializing object");
        var result = _serializer.Serialize(testObject);

        // Assert
        LogAssert("Verifying bytes output");
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
        LogInfo("Serialized bytes count: {0}", result.Length);
    }

    [Fact]
    public void Serialize_WithNull_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Preparing null input");
        TestDto? nullObject = null;

        // Act
        LogAct("Serializing null");
        var result = _serializer.Serialize(nullObject);

        // Assert
        LogAssert("Verifying null result");
        result.ShouldBeNull();
        LogInfo("Null input correctly returned null");
    }

    [Fact]
    public void Serialize_WithType_ShouldReturnBytes()
    {
        // Arrange
        LogArrange("Creating test object");
        var testObject = new TestDto { Name = "TypeTest", Value = 100 };

        // Act
        LogAct("Serializing with explicit type");
        var result = _serializer.Serialize(testObject, typeof(TestDto));

        // Assert
        LogAssert("Verifying bytes output");
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
        LogInfo("Serialized with type bytes count: {0}", result.Length);
    }

    #endregion

    #region SerializeAsync Tests

    [Fact]
    public async Task SerializeAsync_WithValidObject_ShouldReturnBytes()
    {
        // Arrange
        LogArrange("Creating test object");
        var testObject = new TestDto { Name = "Async", Value = 99 };

        // Act
        LogAct("Serializing asynchronously");
        var result = await _serializer.SerializeAsync(testObject, CancellationToken.None);

        // Assert
        LogAssert("Verifying bytes output");
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
        LogInfo("Async serialized bytes count: {0}", result.Length);
    }

    [Fact]
    public async Task SerializeAsync_WithNull_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Preparing null input");
        TestDto? nullObject = null;

        // Act
        LogAct("Serializing null asynchronously");
        var result = await _serializer.SerializeAsync(nullObject, CancellationToken.None);

        // Assert
        LogAssert("Verifying null result");
        result.ShouldBeNull();
        LogInfo("Async null correctly returned null");
    }

    #endregion

    #region SerializeToStream Tests

    [Fact]
    public void SerializeToStream_WithValidObject_ShouldWriteToStream()
    {
        // Arrange
        LogArrange("Creating test object and stream");
        var testObject = new TestDto { Name = "Stream", Value = 55 };
        using var stream = new MemoryStream();

        // Act
        LogAct("Serializing to stream");
        _serializer.SerializeToStream(testObject, stream);

        // Assert
        LogAssert("Verifying stream content");
        stream.Length.ShouldBeGreaterThan(0);
        LogInfo("Stream length: {0}", stream.Length);
    }

    [Fact]
    public void SerializeToStream_WithNull_ShouldNotWriteToStream()
    {
        // Arrange
        LogArrange("Preparing null input and stream");
        TestDto? nullObject = null;
        using var stream = new MemoryStream();

        // Act
        LogAct("Serializing null to stream");
        _serializer.SerializeToStream(nullObject, stream);

        // Assert
        LogAssert("Verifying stream is empty");
        stream.Length.ShouldBe(0);
        LogInfo("Null correctly left stream empty");
    }

    [Fact]
    public void SerializeToStream_WithNullStream_ShouldThrow()
    {
        // Arrange
        LogArrange("Creating test object with null stream");
        var testObject = new TestDto { Name = "NullStream", Value = 1 };

        // Act & Assert
        LogAct("Attempting to serialize to null stream");
        Should.Throw<ArgumentNullException>(() => _serializer.SerializeToStream(testObject, null!));
        LogAssert("ArgumentNullException thrown as expected");
    }

    #endregion

    #region SerializeToStreamAsync Tests

    [Fact]
    public async Task SerializeToStreamAsync_WithValidObject_ShouldWriteToStream()
    {
        // Arrange
        LogArrange("Creating test object and stream");
        var testObject = new TestDto { Name = "AsyncStream", Value = 33 };
        using var stream = new MemoryStream();

        // Act
        LogAct("Serializing to stream asynchronously");
        await _serializer.SerializeToStreamAsync(testObject, stream, CancellationToken.None);

        // Assert
        LogAssert("Verifying stream content");
        stream.Length.ShouldBeGreaterThan(0);
        LogInfo("Async stream length: {0}", stream.Length);
    }

    [Fact]
    public async Task SerializeToStreamAsync_WithNull_ShouldNotWriteToStream()
    {
        // Arrange
        LogArrange("Preparing null input and stream");
        TestDto? nullObject = null;
        using var stream = new MemoryStream();

        // Act
        LogAct("Serializing null to stream asynchronously");
        await _serializer.SerializeToStreamAsync(nullObject, stream, CancellationToken.None);

        // Assert
        LogAssert("Verifying stream is empty");
        stream.Length.ShouldBe(0);
        LogInfo("Async null correctly left stream empty");
    }

    #endregion

    #region Deserialize Tests

    [Fact]
    public void Deserialize_WithValidBytes_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating and serializing test object");
        var original = new TestDto { Name = "Deserialize", Value = 11 };
        var bytes = _serializer.Serialize(original);

        // Act
        LogAct("Deserializing bytes");
        var result = _serializer.Deserialize<TestDto>(bytes);

        // Assert
        LogAssert("Verifying deserialized object");
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Deserialize");
        result.Value.ShouldBe(11);
        LogInfo("Deserialized: Name={0}, Value={1}", result.Name, result.Value);
    }

    [Fact]
    public void Deserialize_WithNull_ShouldReturnDefault()
    {
        // Arrange
        LogArrange("Preparing null input");
        byte[]? nullBytes = null;

        // Act
        LogAct("Deserializing null");
        var result = _serializer.Deserialize<TestDto>(nullBytes);

        // Assert
        LogAssert("Verifying default result");
        result.ShouldBeNull();
        LogInfo("Null correctly returned default");
    }

    [Fact]
    public void Deserialize_WithEmptyArray_ShouldReturnDefault()
    {
        // Arrange
        LogArrange("Preparing empty array");
        var emptyBytes = Array.Empty<byte>();

        // Act
        LogAct("Deserializing empty array");
        var result = _serializer.Deserialize<TestDto>(emptyBytes);

        // Assert
        LogAssert("Verifying default result");
        result.ShouldBeNull();
        LogInfo("Empty array correctly returned default");
    }

    [Fact]
    public void Deserialize_WithType_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating and serializing test object");
        var original = new TestDto { Name = "DeserializeType", Value = 12 };
        var bytes = _serializer.Serialize(original);

        // Act
        LogAct("Deserializing with type");
        var result = _serializer.Deserialize<TestDto>(bytes, typeof(TestDto));

        // Assert
        LogAssert("Verifying deserialized object");
        result.ShouldNotBeNull();
        result.Name.ShouldBe("DeserializeType");
        LogInfo("Deserialized with type: {0}", result.Name);
    }

    [Fact]
    public void Deserialize_NonGeneric_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating and serializing test object");
        var original = new TestDto { Name = "NonGeneric", Value = 13 };
        var bytes = _serializer.Serialize(original);

        // Act
        LogAct("Deserializing non-generic");
        var result = _serializer.Deserialize(bytes, typeof(TestDto));

        // Assert
        LogAssert("Verifying deserialized object");
        result.ShouldNotBeNull();
        result.ShouldBeOfType<TestDto>();
        ((TestDto)result).Name.ShouldBe("NonGeneric");
        LogInfo("Non-generic deserialized: {0}", ((TestDto)result).Name);
    }

    #endregion

    #region DeserializeAsync Tests

    [Fact]
    public async Task DeserializeAsync_WithValidBytes_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating and serializing test object");
        var original = new TestDto { Name = "AsyncDeserialize", Value = 21 };
        var bytes = _serializer.Serialize(original);

        // Act
        LogAct("Deserializing asynchronously");
        var result = await _serializer.DeserializeAsync<TestDto>(bytes, CancellationToken.None);

        // Assert
        LogAssert("Verifying deserialized object");
        result.ShouldNotBeNull();
        result.Name.ShouldBe("AsyncDeserialize");
        LogInfo("Async deserialized: {0}", result.Name);
    }

    [Fact]
    public async Task DeserializeAsync_WithNull_ShouldReturnDefault()
    {
        // Arrange
        LogArrange("Preparing null input");
        byte[]? nullBytes = null;

        // Act
        LogAct("Deserializing null asynchronously");
        var result = await _serializer.DeserializeAsync<TestDto>(nullBytes, CancellationToken.None);

        // Assert
        LogAssert("Verifying default result");
        result.ShouldBeNull();
        LogInfo("Async null correctly returned default");
    }

    #endregion

    #region DeserializeFromStream Tests

    [Fact]
    public void DeserializeFromStream_WithValidStream_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating and serializing test object to stream");
        var original = new TestDto { Name = "FromStream", Value = 31 };
        using var stream = new MemoryStream();
        _serializer.SerializeToStream(original, stream);
        stream.Position = 0;

        // Act
        LogAct("Deserializing from stream");
        var result = _serializer.DeserializeFromStream<TestDto>(stream);

        // Assert
        LogAssert("Verifying deserialized object");
        result.ShouldNotBeNull();
        result.Name.ShouldBe("FromStream");
        LogInfo("Deserialized from stream: {0}", result.Name);
    }

    [Fact]
    public void DeserializeFromStream_WithNullStream_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing null stream");

        // Act & Assert
        LogAct("Attempting to deserialize from null stream");
        Should.Throw<ArgumentNullException>(() => _serializer.DeserializeFromStream<TestDto>(null!));
        LogAssert("ArgumentNullException thrown as expected");
    }

    [Fact]
    public void DeserializeFromStream_WithType_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating and serializing test object to stream");
        var original = new TestDto { Name = "FromStreamType", Value = 32 };
        using var stream = new MemoryStream();
        _serializer.SerializeToStream(original, stream);
        stream.Position = 0;

        // Act
        LogAct("Deserializing from stream with type");
        var result = _serializer.DeserializeFromStream<TestDto>(stream, typeof(TestDto));

        // Assert
        LogAssert("Verifying deserialized object");
        result.ShouldNotBeNull();
        result.Name.ShouldBe("FromStreamType");
        LogInfo("Deserialized from stream with type: {0}", result.Name);
    }

    [Fact]
    public void DeserializeFromStream_NonGeneric_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating and serializing test object to stream");
        var original = new TestDto { Name = "NonGenericStream", Value = 33 };
        using var stream = new MemoryStream();
        _serializer.SerializeToStream(original, stream);
        stream.Position = 0;

        // Act
        LogAct("Deserializing from stream non-generic");
        var result = _serializer.DeserializeFromStream(stream, typeof(TestDto));

        // Assert
        LogAssert("Verifying deserialized object");
        result.ShouldNotBeNull();
        result.ShouldBeOfType<TestDto>();
        ((TestDto)result).Name.ShouldBe("NonGenericStream");
        LogInfo("Non-generic from stream: {0}", ((TestDto)result).Name);
    }

    #endregion

    #region DeserializeFromStreamAsync Tests

    [Fact]
    public async Task DeserializeFromStreamAsync_WithValidStream_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating and serializing test object to stream");
        var original = new TestDto { Name = "AsyncFromStream", Value = 41 };
        using var stream = new MemoryStream();
        _serializer.SerializeToStream(original, stream);
        stream.Position = 0;

        // Act
        LogAct("Deserializing from stream asynchronously");
        var result = await _serializer.DeserializeFromStreamAsync<TestDto>(stream, CancellationToken.None);

        // Assert
        LogAssert("Verifying deserialized object");
        result.ShouldNotBeNull();
        result.Name.ShouldBe("AsyncFromStream");
        LogInfo("Async deserialized from stream: {0}", result.Name);
    }

    [Fact]
    public async Task DeserializeFromStreamAsync_WithNullStream_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing null stream");

        // Act & Assert
        LogAct("Attempting to deserialize async from null stream");
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await _serializer.DeserializeFromStreamAsync<TestDto>(null!, CancellationToken.None));
        LogAssert("ArgumentNullException thrown as expected");
    }

    #endregion

    #region GenerateSchemaDefinition Tests

    [Fact]
    public void GenerateSchemaDefinition_Generic_ShouldReturnSchema()
    {
        // Arrange
        LogArrange("Preparing to generate schema");

        // Act
        LogAct("Generating schema for TestDto");
        var schema = _serializer.GenerateSchemaDefinition<TestDto>();

        // Assert
        LogAssert("Verifying schema content");
        schema.ShouldNotBeNullOrEmpty();
        schema.ShouldContain("TestDto");
        schema.ShouldContain("Name");
        schema.ShouldContain("Value");
        LogInfo("Schema generated: {0}", schema.Substring(0, Math.Min(100, schema.Length)));
    }

    [Fact]
    public void GenerateSchemaDefinition_WithType_ShouldReturnSchema()
    {
        // Arrange
        LogArrange("Preparing to generate schema");

        // Act
        LogAct("Generating schema with type parameter");
        var schema = _serializer.GenerateSchemaDefinition(typeof(TestDto));

        // Assert
        LogAssert("Verifying schema content");
        schema.ShouldNotBeNullOrEmpty();
        schema.ShouldContain("TestDto");
        LogInfo("Schema with type generated");
    }

    #endregion

    #region Options Configuration Tests

    [Fact]
    public void Constructor_ShouldCallConfigureInternal()
    {
        // Arrange & Act
        LogArrange("Creating serializer");
        var serializer = new TestAvroSerializer();

        // Assert
        LogAssert("Verifying ConfigureInternal was called");
        serializer.ConfigureInternalWasCalled.ShouldBeTrue();
        LogInfo("ConfigureInternal was called during construction");
    }

    [Fact]
    public void Options_ShouldBeAccessible()
    {
        // Arrange
        LogArrange("Creating serializer");

        // Act
        LogAct("Accessing options");
        var options = _serializer.Options;

        // Assert
        LogAssert("Verifying options are not null");
        options.ShouldNotBeNull();
        LogInfo("Options accessible");
    }

    #endregion

    #region Round-Trip Tests

    [Fact]
    public void RoundTrip_SerializeDeserialize_ShouldPreserveData()
    {
        // Arrange
        LogArrange("Creating test object");
        var original = new TestDto { Name = "RoundTrip", Value = 999 };

        // Act
        LogAct("Serializing and deserializing");
        var bytes = _serializer.Serialize(original);
        var result = _serializer.Deserialize<TestDto>(bytes);

        // Assert
        LogAssert("Verifying data preserved");
        result.ShouldNotBeNull();
        result.Name.ShouldBe(original.Name);
        result.Value.ShouldBe(original.Value);
        LogInfo("Round-trip successful");
    }

    [Fact]
    public void RoundTrip_StreamSerialization_ShouldPreserveData()
    {
        // Arrange
        LogArrange("Creating test object");
        var original = new TestDto { Name = "StreamRoundTrip", Value = 666 };

        // Act
        LogAct("Serializing to stream and deserializing");
        using var stream = new MemoryStream();
        _serializer.SerializeToStream(original, stream);
        stream.Position = 0;
        var result = _serializer.DeserializeFromStream<TestDto>(stream);

        // Assert
        LogAssert("Verifying data preserved");
        result.ShouldNotBeNull();
        result.Name.ShouldBe(original.Name);
        result.Value.ShouldBe(original.Value);
        LogInfo("Stream round-trip successful");
    }

    [Fact]
    public async Task RoundTrip_AsyncSerialization_ShouldPreserveData()
    {
        // Arrange
        LogArrange("Creating test object");
        var original = new TestDto { Name = "AsyncRoundTrip", Value = 777 };

        // Act
        LogAct("Serializing and deserializing asynchronously");
        var bytes = await _serializer.SerializeAsync(original, CancellationToken.None);
        var result = await _serializer.DeserializeAsync<TestDto>(bytes, CancellationToken.None);

        // Assert
        LogAssert("Verifying data preserved");
        result.ShouldNotBeNull();
        result.Name.ShouldBe(original.Name);
        result.Value.ShouldBe(original.Value);
        LogInfo("Async round-trip successful");
    }

    [Fact]
    public void RoundTrip_ComplexObject_ShouldPreserveAllFields()
    {
        // Arrange
        LogArrange("Creating complex test object");
        var original = new ComplexDto
        {
            Id = 123,
            Name = "Complex",
            Amount = 45.67m,
            IsActive = true,
            CreatedAt = new DateTime(2024, 6, 15, 10, 30, 0),
            UniqueId = Guid.NewGuid()
        };

        // Act
        LogAct("Serializing and deserializing complex object");
        var bytes = _serializer.Serialize(original);
        var result = _serializer.Deserialize<ComplexDto>(bytes);

        // Assert
        LogAssert("Verifying all fields preserved");
        result.ShouldNotBeNull();
        result.Id.ShouldBe(original.Id);
        result.Name.ShouldBe(original.Name);
        result.Amount.ShouldBe(original.Amount);
        result.IsActive.ShouldBe(original.IsActive);
        result.CreatedAt.ShouldBe(original.CreatedAt);
        result.UniqueId.ShouldBe(original.UniqueId);
        LogInfo("Complex round-trip successful");
    }

    #endregion

    #region Test Helpers

    public class TestDto
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    public class ComplexDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UniqueId { get; set; }
    }

    private class TestAvroSerializer : AvroSerializerBase
    {
        public bool ConfigureInternalWasCalled { get; private set; }

        protected override void ConfigureInternal(Options options)
        {
            ConfigureInternalWasCalled = true;
        }
    }

    #endregion
}
