using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace Bedrock.BuildingBlocks.Web.WebApi.HealthChecks;

public static class HealthCheckEndpointExtensions
{
    // Mapeia os endpoints de health check com base nas opcoes registradas.
    // Cada probe e mapeado no seu path com filtro por tag, garantindo que
    // cada endpoint responda apenas ao seu health check correspondente.
    // O response writer serializa o resultado em JSON com recursos informativos.
    public static WebApplication MapBedrockHealthChecks(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<BedrockHealthCheckOptions>();

        if (options.StartupPath is not null)
        {
            app.MapHealthChecks(options.StartupPath, new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains(BedrockHealthCheckOptions.StartupTag),
                ResponseWriter = BedrockHealthCheckResponseWriter.WriteAsync
            });
        }

        if (options.ReadinessPath is not null)
        {
            app.MapHealthChecks(options.ReadinessPath, new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains(BedrockHealthCheckOptions.ReadinessTag),
                ResponseWriter = BedrockHealthCheckResponseWriter.WriteAsync
            });
        }

        if (options.LivenessPath is not null)
        {
            app.MapHealthChecks(options.LivenessPath, new HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains(BedrockHealthCheckOptions.LivenessTag),
                ResponseWriter = BedrockHealthCheckResponseWriter.WriteAsync
            });
        }

        return app;
    }
}
