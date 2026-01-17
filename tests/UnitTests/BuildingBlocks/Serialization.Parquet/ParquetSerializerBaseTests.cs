using System.Text;
using Bedrock.BuildingBlocks.Serialization.Parquet;
using Bedrock.BuildingBlocks.Serialization.Parquet.Models;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Serialization.Parquet;

public class ParquetSerializerBaseTests : TestBase
{
    private readonly TestParquetSerializer _serializer;

    public ParquetSerializerBaseTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _serializer = new TestParquetSerializer();
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

    #region SerializeCollection Tests

    [Fact]
    public void SerializeCollection_WithValidCollection_ShouldReturnBytes()
    {
        // Arrange
        LogArrange("Creating test collection");
        var collection = new[]
        {
            new TestDto { Name = "Item1", Value = 1 },
            new TestDto { Name = "Item2", Value = 2 },
            new TestDto { Name = "Item3", Value = 3 }
        };

        // Act
        LogAct("Serializing collection");
        var result = _serializer.SerializeCollection(collection);

        // Assert
        LogAssert("Verifying bytes output");
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
        LogInfo("Serialized collection bytes count: {0}", result.Length);
    }

    [Fact]
    public void SerializeCollection_WithNull_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Preparing null collection");
        IEnumerable<TestDto>? nullCollection = null;

        // Act
        LogAct("Serializing null collection");
        var result = _serializer.SerializeCollection(nullCollection);

        // Assert
        LogAssert("Verifying null result");
        result.ShouldBeNull();
        LogInfo("Null collection correctly returned null");
    }

    [Fact]
    public void SerializeCollection_WithEmptyCollection_ShouldReturnBytes()
    {
        // Arrange
        LogArrange("Creating empty collection");
        var collection = Array.Empty<TestDto>();

        // Act
        LogAct("Serializing empty collection");
        var result = _serializer.SerializeCollection(collection);

        // Assert
        LogAssert("Verifying bytes output for empty collection");
        result.ShouldNotBeNull();
        LogInfo("Empty collection bytes count: {0}", result.Length);
    }

    [Fact]
    public async Task SerializeCollectionAsync_WithValidCollection_ShouldReturnBytes()
    {
        // Arrange
        LogArrange("Creating test collection");
        var collection = new[]
        {
            new TestDto { Name = "AsyncItem1", Value = 1 },
            new TestDto { Name = "AsyncItem2", Value = 2 }
        };

        // Act
        LogAct("Serializing collection asynchronously");
        var result = await _serializer.SerializeCollectionAsync(collection, CancellationToken.None);

        // Assert
        LogAssert("Verifying bytes output");
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
        LogInfo("Async serialized collection bytes count: {0}", result.Length);
    }

    #endregion

    #region SerializeCollectionToStream Tests

    [Fact]
    public void SerializeCollectionToStream_WithValidCollection_ShouldWriteToStream()
    {
        // Arrange
        LogArrange("Creating test collection and stream");
        var collection = new[]
        {
            new TestDto { Name = "StreamItem1", Value = 1 },
            new TestDto { Name = "StreamItem2", Value = 2 }
        };
        using var stream = new MemoryStream();

        // Act
        LogAct("Serializing collection to stream");
        _serializer.SerializeCollectionToStream(collection, stream);

        // Assert
        LogAssert("Verifying stream content");
        stream.Length.ShouldBeGreaterThan(0);
        LogInfo("Stream length: {0}", stream.Length);
    }

    [Fact]
    public void SerializeCollectionToStream_WithNull_ShouldNotWriteToStream()
    {
        // Arrange
        LogArrange("Preparing null collection and stream");
        IEnumerable<TestDto>? nullCollection = null;
        using var stream = new MemoryStream();

        // Act
        LogAct("Serializing null collection to stream");
        _serializer.SerializeCollectionToStream(nullCollection, stream);

        // Assert
        LogAssert("Verifying stream is empty");
        stream.Length.ShouldBe(0);
        LogInfo("Null collection correctly left stream empty");
    }

    [Fact]
    public void SerializeCollectionToStream_WithNullStream_ShouldThrow()
    {
        // Arrange
        LogArrange("Creating test collection with null stream");
        var collection = new[] { new TestDto { Name = "Test", Value = 1 } };

        // Act & Assert
        LogAct("Attempting to serialize to null stream");
        Should.Throw<ArgumentNullException>(() => _serializer.SerializeCollectionToStream(collection, null!));
        LogAssert("ArgumentNullException thrown as expected");
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

    #region DeserializeCollection Tests

    [Fact]
    public void DeserializeCollection_WithValidBytes_ShouldReturnCollection()
    {
        // Arrange
        LogArrange("Creating and serializing test collection");
        var original = new[]
        {
            new TestDto { Name = "Item1", Value = 1 },
            new TestDto { Name = "Item2", Value = 2 },
            new TestDto { Name = "Item3", Value = 3 }
        };
        var bytes = _serializer.SerializeCollection(original);

        // Act
        LogAct("Deserializing collection");
        var result = _serializer.DeserializeCollection<TestDto>(bytes);

        // Assert
        LogAssert("Verifying deserialized collection");
        result.ShouldNotBeNull();
        var resultList = result.ToList();
        resultList.Count.ShouldBe(3);
        resultList[0].Name.ShouldBe("Item1");
        resultList[1].Name.ShouldBe("Item2");
        resultList[2].Name.ShouldBe("Item3");
        LogInfo("Deserialized collection count: {0}", resultList.Count);
    }

    [Fact]
    public void DeserializeCollection_WithNull_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Preparing null input");
        byte[]? nullBytes = null;

        // Act
        LogAct("Deserializing null collection");
        var result = _serializer.DeserializeCollection<TestDto>(nullBytes);

        // Assert
        LogAssert("Verifying null result");
        result.ShouldBeNull();
        LogInfo("Null correctly returned null collection");
    }

    [Fact]
    public void DeserializeCollection_WithEmptyArray_ShouldReturnNull()
    {
        // Arrange
        LogArrange("Preparing empty array");
        var emptyBytes = Array.Empty<byte>();

        // Act
        LogAct("Deserializing empty array");
        var result = _serializer.DeserializeCollection<TestDto>(emptyBytes);

        // Assert
        LogAssert("Verifying null result");
        result.ShouldBeNull();
        LogInfo("Empty array correctly returned null");
    }

    [Fact]
    public async Task DeserializeCollectionAsync_WithValidBytes_ShouldReturnCollection()
    {
        // Arrange
        LogArrange("Creating and serializing test collection");
        var original = new[]
        {
            new TestDto { Name = "AsyncItem1", Value = 1 },
            new TestDto { Name = "AsyncItem2", Value = 2 }
        };
        var bytes = _serializer.SerializeCollection(original);

        // Act
        LogAct("Deserializing collection asynchronously");
        var result = await _serializer.DeserializeCollectionAsync<TestDto>(bytes, CancellationToken.None);

        // Assert
        LogAssert("Verifying deserialized collection");
        result.ShouldNotBeNull();
        var resultList = result.ToList();
        resultList.Count.ShouldBe(2);
        LogInfo("Async deserialized collection count: {0}", resultList.Count);
    }

    #endregion

    #region DeserializeCollectionFromStream Tests

    [Fact]
    public void DeserializeCollectionFromStream_WithValidStream_ShouldReturnCollection()
    {
        // Arrange
        LogArrange("Creating and serializing test collection to stream");
        var original = new[]
        {
            new TestDto { Name = "StreamItem1", Value = 1 },
            new TestDto { Name = "StreamItem2", Value = 2 }
        };
        using var stream = new MemoryStream();
        _serializer.SerializeCollectionToStream(original, stream);
        stream.Position = 0;

        // Act
        LogAct("Deserializing collection from stream");
        var result = _serializer.DeserializeCollectionFromStream<TestDto>(stream);

        // Assert
        LogAssert("Verifying deserialized collection");
        result.ShouldNotBeNull();
        var resultList = result.ToList();
        resultList.Count.ShouldBe(2);
        resultList[0].Name.ShouldBe("StreamItem1");
        LogInfo("Deserialized collection from stream count: {0}", resultList.Count);
    }

    [Fact]
    public void DeserializeCollectionFromStream_WithNullStream_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing null stream");

        // Act & Assert
        LogAct("Attempting to deserialize from null stream");
        Should.Throw<ArgumentNullException>(() => _serializer.DeserializeCollectionFromStream<TestDto>(null!));
        LogAssert("ArgumentNullException thrown as expected");
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
    public void DeserializeFromStream_NonGenericWithNullStream_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing null stream");

        // Act & Assert
        LogAct("Attempting to deserialize non-generic from null stream");
        Should.Throw<ArgumentNullException>(() => _serializer.DeserializeFromStream(null!, typeof(TestDto)));
        LogAssert("ArgumentNullException thrown as expected");
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
    public void Deserialize_WithTypeAndEmptyArray_ShouldReturnDefault()
    {
        // Arrange
        LogArrange("Preparing empty array");
        var emptyBytes = Array.Empty<byte>();

        // Act
        LogAct("Deserializing empty array with type");
        var result = _serializer.Deserialize<TestDto>(emptyBytes, typeof(TestDto));

        // Assert
        LogAssert("Verifying default result");
        result.ShouldBeNull();
        LogInfo("Empty array with type correctly returned default");
    }

    [Fact]
    public void Deserialize_NonGenericWithEmptyArray_ShouldReturnDefault()
    {
        // Arrange
        LogArrange("Preparing empty array");
        var emptyBytes = Array.Empty<byte>();

        // Act
        LogAct("Deserializing empty array non-generic");
        var result = _serializer.Deserialize(emptyBytes, typeof(TestDto));

        // Assert
        LogAssert("Verifying default result");
        result.ShouldBeNull();
        LogInfo("Non-generic empty array correctly returned default");
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

    [Fact]
    public async Task DeserializeAsync_WithType_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating and serializing test object");
        var original = new TestDto { Name = "AsyncDeserializeType", Value = 22 };
        var bytes = _serializer.Serialize(original);

        // Act
        LogAct("Deserializing asynchronously with type");
        var result = await _serializer.DeserializeAsync<TestDto>(bytes, typeof(TestDto), CancellationToken.None);

        // Assert
        LogAssert("Verifying deserialized object");
        result.ShouldNotBeNull();
        result.Name.ShouldBe("AsyncDeserializeType");
        LogInfo("Async deserialized with type: {0}", result.Name);
    }

    [Fact]
    public async Task DeserializeAsync_NonGeneric_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating and serializing test object");
        var original = new TestDto { Name = "AsyncNonGeneric", Value = 23 };
        var bytes = _serializer.Serialize(original);

        // Act
        LogAct("Deserializing asynchronously non-generic");
        var result = await _serializer.DeserializeAsync(bytes, typeof(TestDto), CancellationToken.None);

        // Assert
        LogAssert("Verifying deserialized object");
        result.ShouldNotBeNull();
        result.ShouldBeOfType<TestDto>();
        ((TestDto)result).Name.ShouldBe("AsyncNonGeneric");
        LogInfo("Async non-generic deserialized: {0}", ((TestDto)result).Name);
    }

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

    [Fact]
    public async Task DeserializeFromStreamAsync_WithType_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating and serializing test object to stream");
        var original = new TestDto { Name = "AsyncFromStreamType", Value = 42 };
        using var stream = new MemoryStream();
        _serializer.SerializeToStream(original, stream);
        stream.Position = 0;

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
    public async Task DeserializeFromStreamAsync_NonGeneric_ShouldReturnObject()
    {
        // Arrange
        LogArrange("Creating and serializing test object to stream");
        var original = new TestDto { Name = "AsyncNonGenericStream", Value = 43 };
        using var stream = new MemoryStream();
        _serializer.SerializeToStream(original, stream);
        stream.Position = 0;

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
    public async Task SerializeAsync_WithType_ShouldReturnBytes()
    {
        // Arrange
        LogArrange("Creating test object");
        var testObject = new TestDto { Name = "AsyncType", Value = 88 };

        // Act
        LogAct("Serializing asynchronously with type");
        var result = await _serializer.SerializeAsync(testObject, typeof(TestDto), CancellationToken.None);

        // Assert
        LogAssert("Verifying bytes output");
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
        LogInfo("Async serialized with type bytes count: {0}", result.Length);
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
        stream.Length.ShouldBeGreaterThan(0);
        LogInfo("Stream with type length: {0}", stream.Length);
    }

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
    public async Task SerializeToStreamAsync_WithType_ShouldWriteToStream()
    {
        // Arrange
        LogArrange("Creating test object and stream");
        var testObject = new TestDto { Name = "AsyncStreamType", Value = 34 };
        using var stream = new MemoryStream();

        // Act
        LogAct("Serializing to stream asynchronously with type");
        await _serializer.SerializeToStreamAsync(testObject, typeof(TestDto), stream, CancellationToken.None);

        // Assert
        LogAssert("Verifying stream content");
        stream.Length.ShouldBeGreaterThan(0);
        LogInfo("Async stream with type length: {0}", stream.Length);
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
        // Apache Arrow Schema.ToString() format: "Schema: Num fields=N, Num metadata=N"
        schema.ShouldContain("Schema:");
        schema.ShouldContain("Num fields=2");
        LogInfo("Schema generated: {0}", schema);
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
        LogInfo("Schema with type generated");
    }

    #endregion

    #region Options Configuration Tests

    [Fact]
    public void Constructor_ShouldCallConfigureInternal()
    {
        // Arrange & Act
        LogArrange("Creating serializer");
        var serializer = new TestParquetSerializer();

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
    public void RoundTrip_CollectionSerialization_ShouldPreserveData()
    {
        // Arrange
        LogArrange("Creating test collection");
        var original = new[]
        {
            new TestDto { Name = "CollectionRoundTrip1", Value = 111 },
            new TestDto { Name = "CollectionRoundTrip2", Value = 222 },
            new TestDto { Name = "CollectionRoundTrip3", Value = 333 }
        };

        // Act
        LogAct("Serializing and deserializing collection");
        var bytes = _serializer.SerializeCollection(original);
        var result = _serializer.DeserializeCollection<TestDto>(bytes)?.ToList();

        // Assert
        LogAssert("Verifying collection data preserved");
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result[0].Name.ShouldBe(original[0].Name);
        result[1].Value.ShouldBe(original[1].Value);
        result[2].Name.ShouldBe(original[2].Name);
        LogInfo("Collection round-trip successful");
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

    #endregion

    #region Test Helpers

    public class TestDto
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    private class TestParquetSerializer : ParquetSerializerBase
    {
        public bool ConfigureInternalWasCalled { get; private set; }

        protected override void ConfigureInternal(Options options)
        {
            ConfigureInternalWasCalled = true;
        }
    }

    #endregion
}
