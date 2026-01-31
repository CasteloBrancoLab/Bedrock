namespace Bedrock.BuildingBlocks.Serialization.Json.Schema.Models;

/// <summary>
/// Represents the result of a JSON Schema validation operation.
/// </summary>
public sealed class SchemaValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the validation was successful.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the collection of validation errors. Empty when <see cref="IsValid"/> is true.
    /// </summary>
    public IReadOnlyList<SchemaValidationError> Errors { get; }

    private SchemaValidationResult(bool isValid, IReadOnlyList<SchemaValidationError> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    /// <summary>
    /// Creates a successful validation result with no errors.
    /// </summary>
    /// <returns>A valid <see cref="SchemaValidationResult"/>.</returns>
    public static SchemaValidationResult Valid()
    {
        return new SchemaValidationResult(true, []);
    }

    /// <summary>
    /// Creates a failed validation result with the specified errors.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    /// <returns>An invalid <see cref="SchemaValidationResult"/>.</returns>
    public static SchemaValidationResult Invalid(IReadOnlyList<SchemaValidationError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return new SchemaValidationResult(false, errors);
    }
}
