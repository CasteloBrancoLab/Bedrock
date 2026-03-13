using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing;

namespace Bedrock.BuildingBlocks.Web.WebApi.Conventions;

// Transforma tokens de rota de PascalCase para kebab-case.
// Aplicado como IOutboundParameterTransformer nas route conventions,
// garantindo que todas as rotas geradas automaticamente sigam o padrao
// kebab-case (ex: UserProfiles → user-profiles, AccessTokens → access-tokens).
// Rotas definidas explicitamente via [Route("...")] nao sao afetadas.
internal sealed partial class KebabCaseParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value)
    {
        if (value is not string stringValue || string.IsNullOrEmpty(stringValue))
            return null;

        // Insere hifen antes de letras maiusculas que seguem minusculas ou digitos,
        // e entre sequencias de maiusculas seguidas de minusculas.
        // Ex: "UserProfiles" → "User-Profiles" → "user-profiles"
        //     "HTMLParser" → "HTML-Parser" → "html-parser"
        //     "OAuth2Token" → "O-Auth2-Token" → "o-auth2-token"
        return KebabCaseRegex().Replace(stringValue, "$1-$2").ToLowerInvariant();
    }

    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex KebabCaseRegex();
}
