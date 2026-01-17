using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bedrock.BuildingBlocks.Serialization.Json.Models;

/// <summary>
/// Configuration options for JSON serialization.
/// </summary>
public class Options
{
    // Stryker disable all : Options initialization is infrastructure code - default values are configuration choices not behavioral logic
    /// <summary>
    /// Gets the JSON serializer options.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Inicializacao de opcoes padrao - valores sao escolhas de configuracao, nao logica comportamental")]
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
    // Stryker restore all

    /// <summary>
    /// Configures the JSON serializer options.
    /// </summary>
    /// <param name="jsonSerializerOptions">The options to use.</param>
    /// <returns>This instance for chaining.</returns>
    // Stryker disable all : Fluent API return pattern - return value tested through builder pattern usage
    [ExcludeFromCodeCoverage(Justification = "Padrao fluent API - valor de retorno testado atraves de encadeamento")]
    public Options WithJsonSerializerOptions(JsonSerializerOptions? jsonSerializerOptions)
    {
        JsonSerializerOptions = jsonSerializerOptions ?? new JsonSerializerOptions();

        return this;
    }
    // Stryker restore all
}
