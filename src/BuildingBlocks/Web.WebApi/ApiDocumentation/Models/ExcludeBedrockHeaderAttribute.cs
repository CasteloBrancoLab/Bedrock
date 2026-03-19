namespace Bedrock.BuildingBlocks.Web.WebApi.ApiDocumentation.Models;

// Remove um ou mais headers do Bedrock da spec OpenAPI para a action ou controller anotada.
// Util quando um endpoint nao precisa de um header especifico (ex: endpoint publico sem tenant).
//
// Uso:
//   [ExcludeBedrockHeader("X-Tenant-Id")]                    — remove um header
//   [ExcludeBedrockHeader("X-Tenant-Id", "X-Correlation-Id")] — remove multiplos
//
// Pode ser aplicado na action (escopo local) ou na controller (escopo global da controller).
// Quando aplicado na controller, afeta todas as actions que nao tenham override individual.
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class ExcludeBedrockHeaderAttribute : Attribute
{
    public string[] HeaderNames { get; }

    public ExcludeBedrockHeaderAttribute(params string[] headerNames)
    {
        HeaderNames = headerNames ?? throw new ArgumentNullException(nameof(headerNames));
    }
}
