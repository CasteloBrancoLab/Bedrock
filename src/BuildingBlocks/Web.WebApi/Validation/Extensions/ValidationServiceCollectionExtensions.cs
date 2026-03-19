using Bedrock.BuildingBlocks.Web.ExecutionContexts;
using Bedrock.BuildingBlocks.Web.WebApi.Validation.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Bedrock.BuildingBlocks.Web.WebApi.Validation.Extensions;

public static class ValidationServiceCollectionExtensions
{
    // Configura o InvalidModelStateResponseFactory do [ApiController]
    // para retornar Problem Details RFC 9457 com correlation ID e formato
    // consistente com o exception handling middleware.
    //
    // Captura erros de model binding ANTES da controller action executar.
    // Complementa o ValidationUtils existente no Core (que e domain-level).
    public static IServiceCollection AddBedrockValidation(
        this IServiceCollection services,
        BedrockValidationOptions? options = null)
    {
        var resolvedOptions = options ?? new BedrockValidationOptions();

        services.Configure<ApiBehaviorOptions>(apiBehavior =>
        {
            apiBehavior.InvalidModelStateResponseFactory = context =>
                CreateValidationProblemDetailsResponse(context, resolvedOptions);
        });

        return services;
    }

    private static IActionResult CreateValidationProblemDetailsResponse(
        ActionContext context,
        BedrockValidationOptions options)
    {
        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Instance = context.HttpContext.Request.Path,
        };

        var correlationId = ExtractCorrelationId(context.HttpContext);
        problemDetails.Extensions["correlationId"] = correlationId;
        problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        options.ConfigureProblemDetails?.Invoke(problemDetails, context.HttpContext);

        return new ObjectResult(problemDetails)
        {
            StatusCode = StatusCodes.Status400BadRequest,
            ContentTypes = { "application/problem+json" },
        };
    }

    private static string ExtractCorrelationId(HttpContext context)
    {
        var headerValue = context.Request.Headers[ExecutionContextFactory.CorrelationIdHeaderName]
            .FirstOrDefault();

        return !string.IsNullOrWhiteSpace(headerValue)
            ? headerValue
            : context.TraceIdentifier;
    }
}
