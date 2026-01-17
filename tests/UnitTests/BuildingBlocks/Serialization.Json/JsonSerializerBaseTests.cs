using System.Text;
using System.Text.Json;
using Bedrock.BuildingBlocks.Serialization.Json;
using Bedrock.BuildingBlocks.Serialization.Json.Models;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Serialization.Json;

public class JsonSerializerBaseTests : TestBase
{
    private readonly TestJsonSerializer _serializer;

    public JsonSerializerBaseTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _serializer = new TestJsonSerializer();
    }

    #region Serialize Tests

    [Fact]
    public void Serialize_WithValidObject_ShouldReturnJsonString()
    {
        // Arrange
        LogArrange("Creating test object");
        var testObject = new TestDto { Name = "Test", Value = 42 };

        // Act
        LogAct("Serializing object");
        var result = _serializer.Serialize(testObject);

        // Assert
        LogAssert("Verifying JSON output");
        result.ShouldNotBeNull();
        result.ShouldContain("\"Name\":\"Test\"");
        result.ShouldContain("\"Value\":42");
        LogInfo("Serialized: {0}", result);
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
    public void Serialize_WithType_ShouldUseProvidedType()
    {
        // Arrange
        LogArrange("Creating test object and type");
        var testObject = new TestDto { Name = "TypeTest", Value = 100 };

        // Act
        LogAct("Serializing with explicit type");
        var result = _serializer.Serialize(testObject, typeof(TestDto));

        // Assert
        LogAssert("Verifying JSON output");
        result.ShouldNotBeNull();
        result.ShouldContain("\"Name\":\"TypeTest\"");
        LogInfo("Serialized with type: {0}", result);
    }

    [Fact]
    public void Serialize_WithNullAndType_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Preparing null input with type");
        TestDto? nullObject = null;

        // Act
        LogAct("Serializing null with type");
        var result = _serializer.Serialize(nullObject, typeof(TestDto));

        // Assert
        LogAssert("Verifying null result");
        result.ShouldBeNull();
        LogInfo("Null with type correctly returned null");
    }

    #endregion

    #region SerializeToUtf8Bytes Tests

    [Fact]
    public void SerializeToUtf8Bytes_WithValidObject_ShouldReturnBytes()
    {
        // Arrange
        LogArrange("Creating test object");
        var testObject = new TestDto { Name = "Bytes", Value = 1 };

        // Act
        LogAct("Serializing to UTF-8 bytes");
        var result = _serializer.SerializeToUtf8Bytes(testObject);

        // Assert
        LogAssert("Verifying byte output");
        result.ShouldNotBeNull();
        var json = Encoding.UTF8.GetString(result);
        json.ShouldContain("\"Name\":\"Bytes\"");
        LogInfo("UTF-8 bytes count: {0}", result.Length);
    }

    [Fact]
    public void SerializeToUtf8Bytes_WithNull_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Preparing null input");
        TestDto? nullObject = null;

        // Act
        LogAct("Serializing null to bytes");
        var result = _serializer.SerializeToUtf8Bytes(nullObject);

        // Assert
        LogAssert("Verifying null result");
        result.ShouldBeNull();
        LogInfo("Null correctly returned null bytes");
    }

    [Fact]
    public void SerializeToUtf8Bytes_WithType_ShouldReturnBytes()
    {
        // Arrange
        LogArrange("Creating test object");
        var testObject = new TestDto { Name = "BytesType", Value = 2 };

        // Act
        LogAct("Serializing to UTF-8 bytes with type");
        var result = _serializer.SerializeToUtf8Bytes(testObject, typeof(TestDto));

        // Assert
        LogAssert("Verifying byte output");
        result.ShouldNotBeNull();
        var json = Encoding.UTF8.GetString(result);
        json.ShouldContain("\"Name\":\"BytesType\"");
        LogInfo("UTF-8 bytes with type count: {0}", result.Length);
    }

    [Fact]
    public void SerializeToUtf8Bytes_WithNullAndType_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Preparing null input");
        TestDto? nullObject = null;

        // Act
        LogAct("Serializing null to bytes with type");
        var result = _serializer.SerializeToUtf8Bytes(nullObject, typeof(TestDto));

        // Assert
        LogAssert("Verifying null result");
        result.ShouldBeNull();
        LogInfo("Null with type correctly returned null bytes");
    }

    #endregion

    #region SerializeAsync Tests

    [Fact]
    public async Task SerializeAsync_WithValidObject_ShouldReturnJsonString()
    {
        // Arrange
        LogArrange("Creating test object");
        var testObject = new TestDto { Name = "Async", Value = 99 };

        // Act
        LogAct("Serializing asynchronously");
        var result = await _serializer.SerializeAsync(testObject, CancellationToken.None);

        // Assert
        LogAssert("Verifying JSON output");
        result.ShouldNotBeNull();
        result.ShouldContain("\"Name\":\"Async\"");
        LogInfo("Async serialized: {0}", result);
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

    [Fact]
    public async Task SerializeAsync_WithType_ShouldReturnJsonString()
    {
        // Arrange
        LogArrange("Creating test object");
        var testObject = new TestDto { Name = "AsyncType", Value = 88 };

        // Act
        LogAct("Serializing asynchronously with type");
        var result = await _serializer.SerializeAsync(testObject, typeof(TestDto), CancellationToken.None);

        // Assert
        LogAssert("Verifying JSON output");
        result.ShouldNotBeNull();
        result.ShouldContain("\"Name\":\"AsyncType\"");
        LogInfo("Async serialized with type: {0}", result);
    }

    [Fact]
    public async Task SerializeAsync_WithNullAndType_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Preparing null input");
        TestDto? nullObject = null;

        // Act
        LogAct("Serializing null asynchronously with type");
        var result = await _serializer.SerializeAsync(nullObject, typeof(TestDto), CancellationToken.None);

        // Assert
        LogAssert("Verifying null result");
        result.ShouldBeNull();
        LogInfo("Async null with type correctly returned null");
    }

    #endregion

    #region SerializeToUtf8BytesAsync Tests

    [Fact]
    public async Task SerializeToUtf8BytesAsync_WithValidObject_ShouldReturnBytes()
    {
        // Arrange
        LogArrange("Creating test object");
        var testObject = new TestDto { Name = "AsyncBytes", Value = 77 };

        // Act
        LogAct("Serializing to bytes asynchronously");
        var result = await _serializer.SerializeToUtf8BytesAsync(testObject, CancellationToken.None);

        // Assert
        LogAssert("Verifying byte output");
        result.ShouldNotBeNull();
        var json = Encoding.UTF8.GetString(result);
        json.ShouldContain("\"Name\":\"AsyncBytes\"");
        LogInfo("Async bytes count: {0}", result.Length);
    }

    [Fact]
    public async Task SerializeToUtf8BytesAsync_WithNull_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Preparing null input");
        TestDto? nullObject = null;

        // Act
        LogAct("Serializing null to bytes asynchronously");
        var result = await _serializer.SerializeToUtf8BytesAsync(nullObject, CancellationToken.None);

        // Assert
        LogAssert("Verifying null result");
        result.ShouldBeNull();
        LogInfo("Async null bytes correctly returned null");
    }

    [Fact]
    public async Task SerializeToUtf8BytesAsync_WithType_ShouldReturnBytes()
    {
        // Arrange
        LogArrange("Creating test object");
        var testObject = new TestDto { Name = "AsyncBytesType", Value = 66 };

        // Act
        LogAct("Serializing to bytes asynchronously with type");
        var result = await _serializer.SerializeToUtf8BytesAsync(testObject, typeof(TestDto), CancellationToken.None);

        // Assert
        LogAssert("Verifying byte output");
        result.ShouldNotBeNull();
        var json = Encoding.UTF8.GetString(result);
        json.ShouldContain("\"Name\":\"AsyncBytesType\"");
        LogInfo("Async bytes with type count: {0}", result.Length);
    }

    [Fact]
    public async Task SerializeToUtf8BytesAsync_WithNullAndType_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Preparing null input");
        TestDto? nullObject = null;

        // Act
        LogAct("Serializing null to bytes asynchronously with type");
        var result = await _serializer.SerializeToUtf8BytesAsync(nullObject, typeof(TestDto), CancellationToken.None);

        // Assert
        LogAssert("Verifying null result");
        result.ShouldBeNull();
        LogInfo("Async null bytes with type correctly returned null");
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
        stream.Position = 0;
        var json = new StreamReader(stream).ReadToEnd();
        json.ShouldContain("\"Name\":\"Stream\"");
        LogInfo("Stream content: {0}", json);
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

    [Fact]
    public void SerializeToStream_WithType_ShouldWriteToStream()
    {
        // Arrange
        LogArrange("Creating test object and stream");
        var testObject = new TestDto { Name = "StreamType", Value = 44 };
        using var stream = new MemoryStream();

        // Act
        LogAct("Serializing to stream with type");
        _serializer.SerializeToStream(testObject, typeof(TestDto), stream);

        // Assert
        LogAssert("Verifying stream content");
        stream.Position = 0;
        var json = new StreamReader(stream).ReadToEnd();
        json.ShouldContain("\"Name\":\"StreamType\"");
        LogInfo("Stream content with type: {0}", json);
    }

    [Fact]
    public void SerializeToStream_WithNullAndType_ShouldNotWriteToStream()
    {
        // Arrange
        LogArrange("Preparing null input with type and stream");
        TestDto? nullObject = null;
        using var stream = new MemoryStream();

        // Act
        LogAct("Serializing null to stream with type");
        _serializer.SerializeToStream(nullObject, typeof(TestDto), stream);

        // Assert
        LogAssert("Verifying stream is empty");
        stream.Length.ShouldBe(0);
        LogInfo("Null with type correctly left stream empty");
    }

    [Fact]
    public void SerializeToStream_WithTypeAndNullStream_ShouldThrow()
    {
        // Arrange
        LogArrange("Creating test object with null stream");
        var testObject = new TestDto { Name = "NullStream", Value = 1 };

        // Act & Assert
        LogAct("Attempting to serialize to null stream with type");
        Should.Throw<ArgumentNullException>(() => _serializer.SerializeToStream(testObject, typeof(TestDto), null!));
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
        stream.Position = 0;
        var json = new StreamReader(stream).ReadToEnd();
        json.ShouldContain("\"Name\":\"AsyncStream\"");
        LogInfo("Async stream content: {0}", json);
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

    [Fact]
    public async Task SerializeToStreamAsync_WithNullStream_ShouldThrow()
    {
        // Arrange
        LogArrange("Creating test object with null stream");
        var testObject = new TestDto { Name = "NullStream", Value = 1 };

        // Act & Assert
        LogAct("Attempting async serialize to null stream");
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await _serializer.SerializeToStreamAsync(testObject, null!, CancellationToken.None));
        LogAssert("ArgumentNullException thrown as expected");
    }

    [Fact]
    public async Task SerializeToStreamAsync_WithType_ShouldWriteToStream()
    {
        // Arrange
        LogArrange("Creating test object and stream");
        var testObject = new TestDto { Name = "AsyncStreamType", Value = 22 };
        using var stream = new MemoryStream();

        // Act
        LogAct("Serializing to stream asynchronously with type");
        await _serializer.SerializeToStreamAsync(testObject, typeof(TestDto), stream, CancellationToken.None);

        // Assert
        LogAssert("Verifying stream content");
        stream.Position = 0;
        var json = new StreamReader(stream).ReadToEnd();
        json.ShouldContain("\"Name\":\"AsyncStreamType\"");
        LogInfo("Async stream content with type: {0}", json);
    }

    [Fact]
    public async Task SerializeToStreamAsync_WithNullAndType_ShouldNotWriteToStream()
    {
        // Arrange
        LogArrange("Preparing null input with type and stream");
        TestDto? nullObject = null;
        using var stream = new MemoryStream();

        // Act
        LogAct("Serializing null to stream asynchronously with type");
        await _serializer.SerializeToStreamAsync(nullObject, typeof(TestDto), stream, CancellationToken.None);

        // Assert
        LogAssert("Verifying stream is empty");
        stream.Length.ShouldBe(0);
        LogInfo("Async null with type correctly left stream empty");
    }

    [Fact]
    public async Task SerializeToStreamAsync_WithTypeAndNullStream_ShouldThrow()
    {
        // Arrange
        LogArrange("Creating test object with null stream");
        var testObject = new TestDto { Name = "NullStream", Value = 1 };

        // Act & Assert
        LogAct("Attempting async serialize to null stream with type");
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await _serializer.SerializeToStreamAsync(testObject, typeof(TestDto), null!, CancellationToken.None));
        LogAssert("ArgumentNullException thrown as expected");
    }

    #endregion

    #region Deserialize Tests

    [Fact]
    public void Deserialize_WithValidJson_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating JSON string");
        var json = """{"Name":"Deserialize","Value":11}""";

        // Act
        LogAct("Deserializing JSON");
        var result = _serializer.Deserialize<TestDto>(json);

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
        string? nullJson = null;

        // Act
        LogAct("Deserializing null");
        var result = _serializer.Deserialize<TestDto>(nullJson);

        // Assert
        LogAssert("Verifying default result");
        result.ShouldBeNull();
        LogInfo("Null correctly returned default");
    }

    [Fact]
    public void Deserialize_WithType_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating JSON string");
        var json = """{"Name":"DeserializeType","Value":12}""";

        // Act
        LogAct("Deserializing with type");
        var result = _serializer.Deserialize<TestDto>(json, typeof(TestDto));

        // Assert
        LogAssert("Verifying deserialized object");
        result.ShouldNotBeNull();
        result.Name.ShouldBe("DeserializeType");
        LogInfo("Deserialized with type: {0}", result.Name);
    }

    [Fact]
    public void Deserialize_WithNullAndType_ShouldReturnDefault()
    {
        // Arrange
        LogArrange("Preparing null input");
        string? nullJson = null;

        // Act
        LogAct("Deserializing null with type");
        var result = _serializer.Deserialize<TestDto>(nullJson, typeof(TestDto));

        // Assert
        LogAssert("Verifying default result");
        result.ShouldBeNull();
        LogInfo("Null with type correctly returned default");
    }

    [Fact]
    public void Deserialize_NonGeneric_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating JSON string");
        var json = """{"Name":"NonGeneric","Value":13}""";

        // Act
        LogAct("Deserializing non-generic");
        var result = _serializer.Deserialize(json, typeof(TestDto));

        // Assert
        LogAssert("Verifying deserialized object");
        result.ShouldNotBeNull();
        result.ShouldBeOfType<TestDto>();
        ((TestDto)result).Name.ShouldBe("NonGeneric");
        LogInfo("Non-generic deserialized: {0}", ((TestDto)result).Name);
    }

    [Fact]
    public void Deserialize_NonGenericWithNull_ShouldReturnDefault()
    {
        // Arrange
        LogArrange("Preparing null input");
        string? nullJson = null;

        // Act
        LogAct("Deserializing null non-generic");
        var result = _serializer.Deserialize(nullJson, typeof(TestDto));

        // Assert
        LogAssert("Verifying default result");
        result.ShouldBeNull();
        LogInfo("Non-generic null correctly returned default");
    }

    #endregion

    #region DeserializeAsync Tests

    [Fact]
    public async Task DeserializeAsync_WithValidJson_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating JSON string");
        var json = """{"Name":"AsyncDeserialize","Value":21}""";

        // Act
        LogAct("Deserializing asynchronously");
        var result = await _serializer.DeserializeAsync<TestDto>(json, CancellationToken.None);

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
        string? nullJson = null;

        // Act
        LogAct("Deserializing null asynchronously");
        var result = await _serializer.DeserializeAsync<TestDto>(nullJson, CancellationToken.None);

        // Assert
        LogAssert("Verifying default result");
        result.ShouldBeNull();
        LogInfo("Async null correctly returned default");
    }

    [Fact]
    public async Task DeserializeAsync_WithType_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating JSON string");
        var json = """{"Name":"AsyncDeserializeType","Value":22}""";

        // Act
        LogAct("Deserializing asynchronously with type");
        var result = await _serializer.DeserializeAsync<TestDto>(json, typeof(TestDto), CancellationToken.None);

        // Assert
        LogAssert("Verifying deserialized object");
        result.ShouldNotBeNull();
        result.Name.ShouldBe("AsyncDeserializeType");
        LogInfo("Async deserialized with type: {0}", result.Name);
    }

    [Fact]
    public async Task DeserializeAsync_WithNullAndType_ShouldReturnDefault()
    {
        // Arrange
        LogArrange("Preparing null input");
        string? nullJson = null;

        // Act
        LogAct("Deserializing null asynchronously with type");
        var result = await _serializer.DeserializeAsync<TestDto>(nullJson, typeof(TestDto), CancellationToken.None);

        // Assert
        LogAssert("Verifying default result");
        result.ShouldBeNull();
        LogInfo("Async null with type correctly returned default");
    }

    [Fact]
    public async Task DeserializeAsync_NonGeneric_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating JSON string");
        var json = """{"Name":"AsyncNonGeneric","Value":23}""";

        // Act
        LogAct("Deserializing asynchronously non-generic");
        var result = await _serializer.DeserializeAsync(json, typeof(TestDto), CancellationToken.None);

        // Assert
        LogAssert("Verifying deserialized object");
        result.ShouldNotBeNull();
        result.ShouldBeOfType<TestDto>();
        ((TestDto)result).Name.ShouldBe("AsyncNonGeneric");
        LogInfo("Async non-generic deserialized: {0}", ((TestDto)result).Name);
    }

    [Fact]
    public async Task DeserializeAsync_NonGenericWithNull_ShouldReturnDefault()
    {
        // Arrange
        LogArrange("Preparing null input");
        string? nullJson = null;

        // Act
        LogAct("Deserializing null asynchronously non-generic");
        var result = await _serializer.DeserializeAsync(nullJson, typeof(TestDto), CancellationToken.None);

        // Assert
        LogAssert("Verifying default result");
        result.ShouldBeNull();
        LogInfo("Async non-generic null correctly returned default");
    }

    #endregion

    #region DeserializeFromStream Tests

    [Fact]
    public void DeserializeFromStream_WithValidStream_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating stream with JSON");
        var json = """{"Name":"FromStream","Value":31}""";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

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
        LogArrange("Creating stream with JSON");
        var json = """{"Name":"FromStreamType","Value":32}""";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

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
    public void DeserializeFromStream_WithTypeAndNullStream_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing null stream");

        // Act & Assert
        LogAct("Attempting to deserialize from null stream with type");
        Should.Throw<ArgumentNullException>(() => _serializer.DeserializeFromStream<TestDto>(null!, typeof(TestDto)));
        LogAssert("ArgumentNullException thrown as expected");
    }

    [Fact]
    public void DeserializeFromStream_NonGeneric_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating stream with JSON");
        var json = """{"Name":"NonGenericStream","Value":33}""";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

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

    [Fact]
    public void DeserializeFromStream_NonGenericWithNullStream_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing null stream");

        // Act & Assert
        LogAct("Attempting to deserialize non-generic from null stream");
        Should.Throw<ArgumentNullException>(() => _serializer.DeserializeFromStream(null!, typeof(TestDto)));
        LogAssert("ArgumentNullException thrown as expected");
    }

    #endregion

    #region DeserializeFromStreamAsync Tests

    [Fact]
    public async Task DeserializeFromStreamAsync_WithValidStream_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating stream with JSON");
        var json = """{"Name":"AsyncFromStream","Value":41}""";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

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

    [Fact]
    public async Task DeserializeFromStreamAsync_WithType_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating stream with JSON");
        var json = """{"Name":"AsyncFromStreamType","Value":42}""";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        // Act
        LogAct("Deserializing from stream asynchronously with type");
        var result = await _serializer.DeserializeFromStreamAsync<TestDto>(stream, typeof(TestDto), CancellationToken.None);

        // Assert
        LogAssert("Verifying deserialized object");
        result.ShouldNotBeNull();
        result.Name.ShouldBe("AsyncFromStreamType");
        LogInfo("Async deserialized from stream with type: {0}", result.Name);
    }

    [Fact]
    public async Task DeserializeFromStreamAsync_WithTypeAndNullStream_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing null stream");

        // Act & Assert
        LogAct("Attempting to deserialize async from null stream with type");
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await _serializer.DeserializeFromStreamAsync<TestDto>(null!, typeof(TestDto), CancellationToken.None));
        LogAssert("ArgumentNullException thrown as expected");
    }

    [Fact]
    public async Task DeserializeFromStreamAsync_NonGeneric_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating stream with JSON");
        var json = """{"Name":"AsyncNonGenericStream","Value":43}""";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        // Act
        LogAct("Deserializing from stream asynchronously non-generic");
        var result = await _serializer.DeserializeFromStreamAsync(stream, typeof(TestDto), CancellationToken.None);

        // Assert
        LogAssert("Verifying deserialized object");
        result.ShouldNotBeNull();
        result.ShouldBeOfType<TestDto>();
        ((TestDto)result).Name.ShouldBe("AsyncNonGenericStream");
        LogInfo("Async non-generic from stream: {0}", ((TestDto)result).Name);
    }

    [Fact]
    public async Task DeserializeFromStreamAsync_NonGenericWithNullStream_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing null stream");

        // Act & Assert
        LogAct("Attempting to deserialize async non-generic from null stream");
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await _serializer.DeserializeFromStreamAsync(null!, typeof(TestDto), CancellationToken.None));
        LogAssert("ArgumentNullException thrown as expected");
    }

    #endregion

    #region DeserializeFromUtf8Bytes Tests

    [Fact]
    public void DeserializeFromUtf8Bytes_WithValidBytes_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating UTF-8 bytes");
        var json = """{"Name":"FromBytes","Value":51}""";
        var bytes = Encoding.UTF8.GetBytes(json);

        // Act
        LogAct("Deserializing from UTF-8 bytes");
        var result = _serializer.DeserializeFromUtf8Bytes<TestDto>(bytes);

        // Assert
        LogAssert("Verifying deserialized object");
        result.ShouldNotBeNull();
        result.Name.ShouldBe("FromBytes");
        LogInfo("Deserialized from bytes: {0}", result.Name);
    }

    [Fact]
    public void DeserializeFromUtf8Bytes_WithNull_ShouldReturnDefault()
    {
        // Arrange
        LogArrange("Preparing null bytes");
        byte[]? nullBytes = null;

        // Act
        LogAct("Deserializing from null bytes");
        var result = _serializer.DeserializeFromUtf8Bytes<TestDto>(nullBytes);

        // Assert
        LogAssert("Verifying default result");
        result.ShouldBeNull();
        LogInfo("Null bytes correctly returned default");
    }

    [Fact]
    public void DeserializeFromUtf8Bytes_WithType_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating UTF-8 bytes");
        var json = """{"Name":"FromBytesType","Value":52}""";
        var bytes = Encoding.UTF8.GetBytes(json);

        // Act
        LogAct("Deserializing from UTF-8 bytes with type");
        var result = _serializer.DeserializeFromUtf8Bytes<TestDto>(bytes, typeof(TestDto));

        // Assert
        LogAssert("Verifying deserialized object");
        result.ShouldNotBeNull();
        result.Name.ShouldBe("FromBytesType");
        LogInfo("Deserialized from bytes with type: {0}", result.Name);
    }

    [Fact]
    public void DeserializeFromUtf8Bytes_WithNullAndType_ShouldReturnDefault()
    {
        // Arrange
        LogArrange("Preparing null bytes");
        byte[]? nullBytes = null;

        // Act
        LogAct("Deserializing from null bytes with type");
        var result = _serializer.DeserializeFromUtf8Bytes<TestDto>(nullBytes, typeof(TestDto));

        // Assert
        LogAssert("Verifying default result");
        result.ShouldBeNull();
        LogInfo("Null bytes with type correctly returned default");
    }

    #endregion

    #region DeserializeFromUtf8BytesAsync Tests

    [Fact]
    public async Task DeserializeFromUtf8BytesAsync_WithValidBytes_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating UTF-8 bytes");
        var json = """{"Name":"AsyncFromBytes","Value":61}""";
        var bytes = Encoding.UTF8.GetBytes(json);

        // Act
        LogAct("Deserializing from UTF-8 bytes asynchronously");
        var result = await _serializer.DeserializeFromUtf8BytesAsync<TestDto>(bytes, CancellationToken.None);

        // Assert
        LogAssert("Verifying deserialized object");
        result.ShouldNotBeNull();
        result.Name.ShouldBe("AsyncFromBytes");
        LogInfo("Async deserialized from bytes: {0}", result.Name);
    }

    [Fact]
    public async Task DeserializeFromUtf8BytesAsync_WithNull_ShouldReturnDefault()
    {
        // Arrange
        LogArrange("Preparing null bytes");
        byte[]? nullBytes = null;

        // Act
        LogAct("Deserializing from null bytes asynchronously");
        var result = await _serializer.DeserializeFromUtf8BytesAsync<TestDto>(nullBytes, CancellationToken.None);

        // Assert
        LogAssert("Verifying default result");
        result.ShouldBeNull();
        LogInfo("Async null bytes correctly returned default");
    }

    [Fact]
    public async Task DeserializeFromUtf8BytesAsync_WithType_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating UTF-8 bytes");
        var json = """{"Name":"AsyncFromBytesType","Value":62}""";
        var bytes = Encoding.UTF8.GetBytes(json);

        // Act
        LogAct("Deserializing from UTF-8 bytes asynchronously with type");
        var result = await _serializer.DeserializeFromUtf8BytesAsync<TestDto>(bytes, typeof(TestDto), CancellationToken.None);

        // Assert
        LogAssert("Verifying deserialized object");
        result.ShouldNotBeNull();
        result.Name.ShouldBe("AsyncFromBytesType");
        LogInfo("Async deserialized from bytes with type: {0}", result.Name);
    }

    [Fact]
    public async Task DeserializeFromUtf8BytesAsync_WithNullAndType_ShouldReturnDefault()
    {
        // Arrange
        LogArrange("Preparing null bytes");
        byte[]? nullBytes = null;

        // Act
        LogAct("Deserializing from null bytes asynchronously with type");
        var result = await _serializer.DeserializeFromUtf8BytesAsync<TestDto>(nullBytes, typeof(TestDto), CancellationToken.None);

        // Assert
        LogAssert("Verifying default result");
        result.ShouldBeNull();
        LogInfo("Async null bytes with type correctly returned default");
    }

    #endregion

    #region Options Configuration Tests

    [Fact]
    public void Constructor_ShouldCallConfigureInternal()
    {
        // Arrange & Act
        LogArrange("Creating serializer");
        var serializer = new TestJsonSerializer();

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

    [Fact]
    public void Serialize_WithCustomOptions_ShouldUseOptions()
    {
        // Arrange
        LogArrange("Creating serializer with camelCase naming");
        var serializer = new CamelCaseJsonSerializer();
        var testObject = new TestDto { Name = "CustomOptions", Value = 100 };

        // Act
        LogAct("Serializing with custom options");
        var result = serializer.Serialize(testObject);

        // Assert
        LogAssert("Verifying camelCase naming");
        result.ShouldNotBeNull();
        result.ShouldContain("\"name\":");
        result.ShouldContain("\"value\":");
        LogInfo("Custom options applied: {0}", result);
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
        var json = _serializer.Serialize(original);
        var result = _serializer.Deserialize<TestDto>(json);

        // Assert
        LogAssert("Verifying data preserved");
        result.ShouldNotBeNull();
        result.Name.ShouldBe(original.Name);
        result.Value.ShouldBe(original.Value);
        LogInfo("Round-trip successful");
    }

    [Fact]
    public void RoundTrip_BytesSerialization_ShouldPreserveData()
    {
        // Arrange
        LogArrange("Creating test object");
        var original = new TestDto { Name = "BytesRoundTrip", Value = 888 };

        // Act
        LogAct("Serializing to bytes and deserializing");
        var bytes = _serializer.SerializeToUtf8Bytes(original);
        var result = _serializer.DeserializeFromUtf8Bytes<TestDto>(bytes);

        // Assert
        LogAssert("Verifying data preserved");
        result.ShouldNotBeNull();
        result.Name.ShouldBe(original.Name);
        result.Value.ShouldBe(original.Value);
        LogInfo("Bytes round-trip successful");
    }

    [Fact]
    public async Task RoundTrip_AsyncSerialization_ShouldPreserveData()
    {
        // Arrange
        LogArrange("Creating test object");
        var original = new TestDto { Name = "AsyncRoundTrip", Value = 777 };

        // Act
        LogAct("Serializing and deserializing asynchronously");
        var json = await _serializer.SerializeAsync(original, CancellationToken.None);
        var result = await _serializer.DeserializeAsync<TestDto>(json, CancellationToken.None);

        // Assert
        LogAssert("Verifying data preserved");
        result.ShouldNotBeNull();
        result.Name.ShouldBe(original.Name);
        result.Value.ShouldBe(original.Value);
        LogInfo("Async round-trip successful");
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

    #endregion

    #region Test Helpers

    private class TestDto
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    private class TestJsonSerializer : JsonSerializerBase
    {
        public bool ConfigureInternalWasCalled { get; private set; }

        protected override void ConfigureInternal(Options options)
        {
            ConfigureInternalWasCalled = true;
        }
    }

    private class CamelCaseJsonSerializer : JsonSerializerBase
    {
        protected override void ConfigureInternal(Options options)
        {
            options.WithJsonSerializerOptions(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
    }

    #endregion
}
