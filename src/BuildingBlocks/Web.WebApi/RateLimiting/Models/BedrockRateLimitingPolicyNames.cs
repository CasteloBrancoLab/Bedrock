namespace Bedrock.BuildingBlocks.Web.WebApi.RateLimiting.Models;

// Nomes das politicas built-in do Bedrock para uso com [EnableRateLimiting].
// Politicas de rota usam nomes customizados definidos pelo consumidor.
public static class BedrockRateLimitingPolicyNames
{
    public const string Global = "bedrock-global";
    public const string Tenant = "bedrock-tenant";
}
