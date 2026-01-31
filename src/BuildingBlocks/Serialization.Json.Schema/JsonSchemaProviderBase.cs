using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Bedrock.BuildingBlocks.Serialization.Json.Schema.Interfaces;
using Bedrock.BuildingBlocks.Serialization.Json.Schema.Models;
using Json.Schema;
using Json.Schema.Generation;

namespace Bedrock.BuildingBlocks.Serialization.Json.Schema;

/// <summary>
/// Abstract base class for JSON Schema providers using JsonSchema.Net.
/// </summary>
public abstract class JsonSchemaProviderBase : IJsonSchemaProvider
{
    private readonly SchemaGeneratorConfiguration _generatorConfiguration;
    private readonly EvaluationOptions _evaluationOptions;
    private readonly JsonSerializerOptions _serializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonSchemaProviderBase"/> class.
    /// </summary>
    protected JsonSchemaProviderBase()
    {
        _generatorConfiguration = new SchemaGeneratorConfiguration();
        _evaluationOptions = new EvaluationOptions
        {
            OutputFormat = OutputFormat.List,
        };
        _serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        ConfigureInternal(_generatorConfiguration, _evaluationOptions, _serializerOptions);
    }

    /// <inheritdoc />
    public JsonSchema GenerateSchema<T>()
    {
        return GenerateSchema(typeof(T));
    }

    /// <inheritdoc />
    public JsonSchema GenerateSchema(Type type)
    {
        // Stryker disable once Statement : Guard clause - downstream code also throws on null but with different exception type
        ArgumentNullException.ThrowIfNull(type);

        JsonSchemaBuilder builder = new JsonSchemaBuilder().FromType(type, _generatorConfiguration);
        return builder.Build();
    }

    /// <inheritdoc />
    public string ExportSchema<T>()
    {
        return ExportSchema(typeof(T));
    }

    /// <inheritdoc />
    public string ExportSchema(Type type)
    {
        // Stryker disable once Statement : Guard clause - downstream code also throws on null but with different exception type
        ArgumentNullException.ThrowIfNull(type);

        JsonSchema schema = GenerateSchema(type);
        return JsonSerializer.Serialize(schema, _serializerOptions);
    }

    /// <inheritdoc />
    public async Task ExportSchemaToStreamAsync<T>(Stream destination, CancellationToken cancellationToken = default)
    {
        await ExportSchemaToStreamAsync(typeof(T), destination, cancellationToken);
    }

    /// <inheritdoc />
    public async Task ExportSchemaToStreamAsync(Type type, Stream destination, CancellationToken cancellationToken = default)
    {
        // Stryker disable once Statement : Guard clause - downstream code also throws on null but with different exception type
        ArgumentNullException.ThrowIfNull(type);
        // Stryker disable once Statement : Guard clause - downstream code also throws on null but with different exception type
        ArgumentNullException.ThrowIfNull(destination);

        JsonSchema schema = GenerateSchema(type);
        await JsonSerializer.SerializeAsync(destination, schema, _serializerOptions, cancellationToken);
    }

    /// <inheritdoc />
    public SchemaValidationResult Validate<T>(string json)
    {
        // Stryker disable once Statement : Guard clause - downstream code also throws on null but with different exception type
        ArgumentNullException.ThrowIfNull(json);

        JsonSchema schema = GenerateSchema<T>();
        return Validate(json, schema);
    }

    /// <inheritdoc />
    public SchemaValidationResult Validate(string json, JsonSchema schema)
    {
        // Stryker disable once Statement : Guard clause - downstream code also throws on null but with different exception type
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(schema);

        using JsonDocument document = JsonDocument.Parse(json);
        EvaluationResults results = schema.Evaluate(document.RootElement, _evaluationOptions);

        if (results.IsValid)
            return SchemaValidationResult.Valid();

        List<SchemaValidationError> errors = ExtractErrors(results);
        return SchemaValidationResult.Invalid(errors);
    }

    /// <summary>
    /// Override this method to configure the schema generator, evaluation and serializer options.
    /// </summary>
    /// <param name="generatorConfiguration">The schema generator configuration.</param>
    /// <param name="evaluationOptions">The evaluation options for validation.</param>
    /// <param name="serializerOptions">The JSON serializer options for schema export.</param>
    protected abstract void ConfigureInternal(
        SchemaGeneratorConfiguration generatorConfiguration,
        EvaluationOptions evaluationOptions,
        JsonSerializerOptions serializerOptions);

    // Stryker disable all : Error extraction depends on JsonSchema.Net internal EvaluationResults structure - branches vary by OutputFormat and schema complexity
    [ExcludeFromCodeCoverage(Justification = "Extracao de erros depende da estrutura interna de EvaluationResults do JsonSchema.Net - branches variam por OutputFormat e complexidade do schema")]
    private static List<SchemaValidationError> ExtractErrors(EvaluationResults results)
    {
        List<SchemaValidationError> errors = [];

        if (results.Details is null || results.Details.Count == 0)
        {
            if (results.Errors is not null)
            {
                foreach (var kvp in results.Errors)
                {
                    errors.Add(new SchemaValidationError(
                        results.InstanceLocation.ToString(),
                        kvp.Value));
                }
            }

            return errors;
        }

        foreach (EvaluationResults detail in results.Details)
        {
            if (detail.IsValid)
                continue;

            if (detail.Errors is not null)
            {
                foreach (var kvp in detail.Errors)
                {
                    errors.Add(new SchemaValidationError(
                        detail.InstanceLocation.ToString(),
                        kvp.Value));
                }
            }

            if (detail.Details is not null && detail.Details.Count > 0)
            {
                errors.AddRange(ExtractErrors(detail));
            }
        }

        return errors;
    }
    // Stryker restore all
}
