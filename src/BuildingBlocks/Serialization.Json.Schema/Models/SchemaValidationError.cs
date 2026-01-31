namespace Bedrock.BuildingBlocks.Serialization.Json.Schema.Models;

/// <summary>
/// Represents a single validation error from JSON Schema validation.
/// </summary>
public sealed class SchemaValidationError
{
    /// <summary>
    /// Gets the path in the JSON document where the error occurred.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Gets the error message describing the validation failure.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaValidationError"/> class.
    /// </summary>
    /// <param name="path">The path in the JSON document where the error occurred.</param>
    /// <param name="message">The error message.</param>
    public SchemaValidationError(string path, string message)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(message);

        Path = path;
        Message = message;
    }
}
