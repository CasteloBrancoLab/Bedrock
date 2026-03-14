using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Bedrock.BuildingBlocks.Web.WebApi.Cors;

// Configuracao fluente para CORS do Bedrock.
//
// Suporta multiplas politicas nomeadas e uma politica default.
// A politica default e aplicada globalmente a todos os endpoints
// que nao tenham [EnableCors("policy")] ou [DisableCors] explicito.
//
// Uso tipico:
//   new BedrockCorsOptions()
//       .AddPolicy("public-api", policy => policy
//           .WithOrigins("https://app.example.com", "https://admin.example.com")
//           .WithMethods("GET", "POST", "PUT", "DELETE")
//           .WithHeaders("Content-Type", "Authorization", "X-Correlation-Id")
//           .WithCredentials()
//           .WithPreflightMaxAge(TimeSpan.FromHours(1)))
//       .SetDefaultPolicy("public-api")
public sealed class BedrockCorsOptions
{
    internal List<BedrockCorsPolicyOptions> Policies { get; } = [];
    internal string? DefaultPolicyName { get; private set; }
    internal Action<CorsOptions>? ConfigureCors { get; private set; }

    // Adiciona uma politica CORS nomeada via fluent API.
    // O nome pode ser referenciado com [EnableCors("policy-name")] em controllers/actions.
    public BedrockCorsOptions AddPolicy(string name, Action<BedrockCorsPolicyOptions> configure)
    {
        var policy = new BedrockCorsPolicyOptions(name);
        configure(policy);
        Policies.Add(policy);
        return this;
    }

    // Define qual politica nomeada sera a default (aplicada globalmente).
    // Deve referenciar uma politica ja adicionada via AddPolicy.
    public BedrockCorsOptions SetDefaultPolicy(string policyName)
    {
        DefaultPolicyName = policyName;
        return this;
    }

    // Callback para estender ou sobrescrever a configuracao do CorsOptions
    // apos os defaults do Bedrock.
    public BedrockCorsOptions Configure(Action<CorsOptions> configure)
    {
        ConfigureCors = configure;
        return this;
    }
}
