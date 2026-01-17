using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bedrock.BuildingBlocks.Serialization.Json.Models;

/// <summary>
/// Configuration options for JSON serialization.
/// </summary>
public class Options
{
    /// <summary>
    /// Gets the JSON serializer options.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions
    {
        get
        {
            field ??= new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            };

            return field;
        }

        private set;
    }

    /// <summary>
    /// Configures the JSON serializer options.
    /// </summary>
    /// <param name="jsonSerializerOptions">The options to use.</param>
    /// <returns>This instance for chaining.</returns>
    public Options WithJsonSerializerOptions(JsonSerializerOptions? jsonSerializerOptions)
    {
        JsonSerializerOptions = jsonSerializerOptions ?? new JsonSerializerOptions();

        return this;
    }
}
