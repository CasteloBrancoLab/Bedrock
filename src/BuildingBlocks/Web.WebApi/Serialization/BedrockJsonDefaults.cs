using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bedrock.BuildingBlocks.Web.WebApi.Serialization;

// Configuracao centralizada de JSON para APIs do Bedrock.
// Aplicada tanto no AddControllers (serialização de responses/requests)
// quanto em qualquer JsonSerializer manual que use estas opcoes.
//
// Convencoes:
// - camelCase para propriedades (padrao universal para JSON APIs)
// - Enums como string (legibilidade e compatibilidade com clients)
// - Numeros lidos de strings JSON (tolerancia a clients que enviam "123" em vez de 123)
// - Propriedades null incluidas no output (contrato explicito, client sabe que o campo existe)
// - Trailing commas e comentarios permitidos na leitura (tolerancia a input humano)
public static class BedrockJsonDefaults
{
    public static void Configure(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNameCaseInsensitive = true;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
        options.ReadCommentHandling = JsonCommentHandling.Skip;
        options.AllowTrailingCommas = true;
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }
}
