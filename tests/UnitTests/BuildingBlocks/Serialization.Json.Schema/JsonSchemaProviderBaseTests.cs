using System.Text;
using System.Text.Json;
using Bedrock.BuildingBlocks.Serialization.Json.Schema;
using Bedrock.BuildingBlocks.Serialization.Json.Schema.Models;
using Bedrock.BuildingBlocks.Testing;
using Json.Schema;
using Json.Schema.Generation;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Serialization.Json.Schema;

public class JsonSchemaProviderBaseTests : TestBase
{
    private readonly TestSchemaProvider _provider;

    public JsonSchemaProviderBaseTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _provider = new TestSchemaProvider();
    }

    #region GenerateSchema Tests

    [Fact]
    public void GenerateSchema_Generic_ShouldReturnSchema()
    {
        // Arrange
        LogArrange("Preparing to generate schema for TestDto");

        // Act
        LogAct("Generating schema");
        var schema = _provider.GenerateSchema<TestDto>();

        // Assert
        LogAssert("Verifying schema");
        schema.ShouldNotBeNull();
        var json = JsonSerializer.Serialize(schema);
        json.ShouldContain("\"type\":\"object\"");
        LogInfo("Schema generated: {0}", json);
    }

    [Fact]
    public void GenerateSchema_WithType_ShouldReturnSchema()
    {
        // Arrange
        LogArrange("Preparing to generate schema from Type");

        // Act
        LogAct("Generating schema with Type parameter");
        var schema = _provider.GenerateSchema(typeof(TestDto));

        // Assert
        LogAssert("Verifying schema");
        schema.ShouldNotBeNull();
        var json = JsonSerializer.Serialize(schema);
        json.ShouldContain("\"type\":\"object\"");
        LogInfo("Schema generated from Type: {0}", json);
    }

    [Fact]
    public void GenerateSchema_WithNullType_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing null type");

        // Act & Assert
        LogAct("Generating schema with null type");
        Should.Throw<ArgumentNullException>(() => _provider.GenerateSchema(null!));
        LogAssert("ArgumentNullException thrown as expected");
    }

    #endregion

    #region ExportSchema Tests

    [Fact]
    public void ExportSchema_Generic_ShouldReturnJsonString()
    {
        // Arrange
        LogArrange("Preparing to export schema for TestDto");

        // Act
        LogAct("Exporting schema");
        var json = _provider.ExportSchema<TestDto>();

        // Assert
        LogAssert("Verifying JSON string");
        json.ShouldNotBeNull();
        json.ShouldContain("\"type\": \"object\"");
        json.ShouldContain("\"properties\"");
        LogInfo("Exported schema: {0}", json);
    }

    [Fact]
    public void ExportSchema_WithType_ShouldReturnJsonString()
    {
        // Arrange
        LogArrange("Preparing to export schema from Type");

        // Act
        LogAct("Exporting schema with Type parameter");
        var json = _provider.ExportSchema(typeof(TestDto));

        // Assert
        LogAssert("Verifying JSON string");
        json.ShouldNotBeNull();
        json.ShouldContain("\"type\": \"object\"");
        LogInfo("Exported schema from Type: {0}", json);
    }

    [Fact]
    public void ExportSchema_WithNullType_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing null type");

        // Act & Assert
        LogAct("Exporting schema with null type");
        Should.Throw<ArgumentNullException>(() => _provider.ExportSchema(null!));
        LogAssert("ArgumentNullException thrown as expected");
    }

    #endregion

    #region ExportSchemaToStreamAsync Tests

    [Fact]
    public async Task ExportSchemaToStreamAsync_Generic_ShouldWriteToStream()
    {
        // Arrange
        LogArrange("Preparing stream");
        using var stream = new MemoryStream();

        // Act
        LogAct("Exporting schema to stream");
        await _provider.ExportSchemaToStreamAsync<TestDto>(stream);

        // Assert
        LogAssert("Verifying stream content");
        stream.Position = 0;
        var json = new StreamReader(stream).ReadToEnd();
        json.ShouldContain("\"type\": \"object\"");
        LogInfo("Stream content: {0}", json);
    }

    [Fact]
    public async Task ExportSchemaToStreamAsync_WithType_ShouldWriteToStream()
    {
        // Arrange
        LogArrange("Preparing stream");
        using var stream = new MemoryStream();

        // Act
        LogAct("Exporting schema to stream with Type");
        await _provider.ExportSchemaToStreamAsync(typeof(TestDto), stream);

        // Assert
        LogAssert("Verifying stream content");
        stream.Position = 0;
        var json = new StreamReader(stream).ReadToEnd();
        json.ShouldContain("\"type\": \"object\"");
        LogInfo("Stream content from Type: {0}", json);
    }

    [Fact]
    public async Task ExportSchemaToStreamAsync_WithNullType_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing null type and stream");
        using var stream = new MemoryStream();

        // Act & Assert
        LogAct("Exporting schema to stream with null type");
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await _provider.ExportSchemaToStreamAsync(null!, stream));
        LogAssert("ArgumentNullException thrown as expected");
    }

    [Fact]
    public async Task ExportSchemaToStreamAsync_WithNullStream_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing null stream");

        // Act & Assert
        LogAct("Exporting schema to null stream");
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await _provider.ExportSchemaToStreamAsync(typeof(TestDto), null!));
        LogAssert("ArgumentNullException thrown as expected");
    }

    #endregion

    #region Validate Tests

    [Fact]
    public void Validate_Generic_WithValidJson_ShouldReturnValid()
    {
        // Arrange
        LogArrange("Creating valid JSON");
        var json = """{"Name":"Test","Value":42}""";

        // Act
        LogAct("Validating JSON");
        var result = _provider.Validate<TestDto>(json);

        // Assert
        LogAssert("Verifying valid result");
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        LogInfo("Validation passed");
    }

    [Fact]
    public void Validate_Generic_WithInvalidJson_ShouldReturnInvalid()
    {
        // Arrange
        LogArrange("Creating invalid JSON (wrong type for Value)");
        var json = """{"Name":"Test","Value":"not-a-number"}""";

        // Act
        LogAct("Validating invalid JSON");
        var result = _provider.Validate<TestDto>(json);

        // Assert
        LogAssert("Verifying invalid result");
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        LogInfo("Validation failed with {0} errors", result.Errors.Count);
    }

    [Fact]
    public void Validate_Generic_WithNullJson_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing null JSON");

        // Act & Assert
        LogAct("Validating null JSON");
        Should.Throw<ArgumentNullException>(() => _provider.Validate<TestDto>(null!));
        LogAssert("ArgumentNullException thrown as expected");
    }

    [Fact]
    public void Validate_WithSchemaAndValidJson_ShouldReturnValid()
    {
        // Arrange
        LogArrange("Creating schema and valid JSON");
        var schema = _provider.GenerateSchema<TestDto>();
        var json = """{"Name":"Test","Value":42}""";

        // Act
        LogAct("Validating with explicit schema");
        var result = _provider.Validate(json, schema);

        // Assert
        LogAssert("Verifying valid result");
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        LogInfo("Validation with explicit schema passed");
    }

    [Fact]
    public void Validate_WithSchemaAndInvalidJson_ShouldReturnInvalid()
    {
        // Arrange
        LogArrange("Creating schema and invalid JSON");
        var schema = _provider.GenerateSchema<TestDto>();
        var json = """{"Name":123,"Value":"wrong"}""";

        // Act
        LogAct("Validating invalid JSON with explicit schema");
        var result = _provider.Validate(json, schema);

        // Assert
        LogAssert("Verifying invalid result");
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
        LogInfo("Validation failed with {0} errors", result.Errors.Count);
    }

    [Fact]
    public void Validate_WithNullJson_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing null JSON and schema");
        var schema = _provider.GenerateSchema<TestDto>();

        // Act & Assert
        LogAct("Validating null JSON with schema");
        Should.Throw<ArgumentNullException>(() => _provider.Validate(null!, schema));
        LogAssert("ArgumentNullException thrown as expected");
    }

    [Fact]
    public void Validate_WithNullSchema_ShouldThrow()
    {
        // Arrange
        LogArrange("Preparing valid JSON and null schema");
        var json = """{"Name":"Test","Value":42}""";

        // Act & Assert
        LogAct("Validating with null schema");
        Should.Throw<ArgumentNullException>(() => _provider.Validate(json, null!));
        LogAssert("ArgumentNullException thrown as expected");
    }

    [Fact]
    public void Validate_WithValidJson_ErrorsShouldHavePathAndMessage()
    {
        // Arrange
        LogArrange("Creating JSON with wrong type");
        var json = """{"Name":123,"Value":"wrong"}""";

        // Act
        LogAct("Validating to check error details");
        var result = _provider.Validate<TestDto>(json);

        // Assert
        LogAssert("Verifying error details");
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldAllBe(e => !string.IsNullOrEmpty(e.Message));
        LogInfo("All errors have non-empty messages");
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void Constructor_ShouldCallConfigureInternal()
    {
        // Arrange & Act
        LogArrange("Creating schema provider");
        var provider = new TestSchemaProvider();

        // Assert
        LogAssert("Verifying ConfigureInternal was called");
        provider.ConfigureInternalWasCalled.ShouldBeTrue();
        LogInfo("ConfigureInternal was called during construction");
    }

    #endregion

    #region Round-Trip Tests

    [Fact]
    public void GenerateSchema_AndExport_ShouldProduceConsistentResults()
    {
        // Arrange
        LogArrange("Preparing schema generation and export");

        // Act
        LogAct("Generating and exporting schema");
        var schema = _provider.GenerateSchema<TestDto>();
        var exported = _provider.ExportSchema<TestDto>();
        var schemaJson = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });

        // Assert
        LogAssert("Verifying consistency");
        exported.ShouldBe(schemaJson);
        LogInfo("Schema generation and export are consistent");
    }

    [Fact]
    public async Task ExportToStream_AndExportToString_ShouldMatch()
    {
        // Arrange
        LogArrange("Preparing stream and string export");
        using var stream = new MemoryStream();

        // Act
        LogAct("Exporting to both stream and string");
        var stringResult = _provider.ExportSchema<TestDto>();
        await _provider.ExportSchemaToStreamAsync<TestDto>(stream);
        stream.Position = 0;
        var streamResult = new StreamReader(stream).ReadToEnd();

        // Assert
        LogAssert("Verifying match");
        streamResult.ShouldBe(stringResult);
        LogInfo("Stream and string exports match");
    }

    #endregion

    #region Test Helpers

    private class TestDto
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    private class TestSchemaProvider : JsonSchemaProviderBase
    {
        public bool ConfigureInternalWasCalled { get; private set; }

        protected override void ConfigureInternal(
            SchemaGeneratorConfiguration generatorConfiguration,
            EvaluationOptions evaluationOptions,
            JsonSerializerOptions serializerOptions)
        {
            ConfigureInternalWasCalled = true;
        }
    }

    #endregion
}
