using Bedrock.BuildingBlocks.Serialization.Json.Schema.Models;
using Json.Schema;

namespace Bedrock.BuildingBlocks.Serialization.Json.Schema.Interfaces;

/// <summary>
/// Interface for JSON Schema generation, export and validation.
/// </summary>
public interface IJsonSchemaProvider
{
    /// <summary>
    /// Generates a JSON Schema from the specified type.
    /// </summary>
    /// <typeparam name="T">The type to generate a schema for.</typeparam>
    /// <returns>The generated <see cref="JsonSchema"/>.</returns>
    JsonSchema GenerateSchema<T>();

    /// <summary>
    /// Generates a JSON Schema from the specified type.
    /// </summary>
    /// <param name="type">The type to generate a schema for.</param>
    /// <returns>The generated <see cref="JsonSchema"/>.</returns>
    JsonSchema GenerateSchema(Type type);

    /// <summary>
    /// Exports the JSON Schema for the specified type as a JSON string.
    /// </summary>
    /// <typeparam name="T">The type to generate a schema for.</typeparam>
    /// <returns>The JSON Schema as a string.</returns>
    string ExportSchema<T>();

    /// <summary>
    /// Exports the JSON Schema for the specified type as a JSON string.
    /// </summary>
    /// <param name="type">The type to generate a schema for.</param>
    /// <returns>The JSON Schema as a string.</returns>
    string ExportSchema(Type type);

    /// <summary>
    /// Exports the JSON Schema for the specified type to a stream.
    /// </summary>
    /// <typeparam name="T">The type to generate a schema for.</typeparam>
    /// <param name="destination">The stream to write the schema to.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ExportSchemaToStreamAsync<T>(Stream destination, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports the JSON Schema for the specified type to a stream.
    /// </summary>
    /// <param name="type">The type to generate a schema for.</param>
    /// <param name="destination">The stream to write the schema to.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ExportSchemaToStreamAsync(Type type, Stream destination, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a JSON string against the schema generated for the specified type.
    /// </summary>
    /// <typeparam name="T">The type to validate against.</typeparam>
    /// <param name="json">The JSON string to validate.</param>
    /// <returns>The validation result.</returns>
    SchemaValidationResult Validate<T>(string json);

    /// <summary>
    /// Validates a JSON string against the specified schema.
    /// </summary>
    /// <param name="json">The JSON string to validate.</param>
    /// <param name="schema">The schema to validate against.</param>
    /// <returns>The validation result.</returns>
    SchemaValidationResult Validate(string json, JsonSchema schema);
}
