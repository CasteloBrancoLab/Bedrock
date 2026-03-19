using Bedrock.BuildingBlocks.Web.ExecutionContexts;
using Bedrock.BuildingBlocks.Web.ExecutionContexts.Interfaces;
using Bedrock.BuildingBlocks.Web.WebApi.Envelope.Factories;
using Bedrock.BuildingBlocks.Web.WebApi.Envelope.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bedrock.BuildingBlocks.Web.WebApi.Controllers;

[ApiController]
public abstract class BedrockApiControllerBase : ControllerBase
{
    private readonly IExecutionContextFactory _executionContextFactory;

    protected BedrockApiControllerBase(IExecutionContextFactory executionContextFactory)
    {
        _executionContextFactory = executionContextFactory ?? throw new ArgumentNullException(nameof(executionContextFactory));
    }

    protected ExecutionContext CreateExecutionContext()
    {
        return _executionContextFactory.Create(HttpContext);
    }

    // Resolve automaticamente o status code a partir do estado do ExecutionContext:
    // - data null ou IsFaulted (sem parcial) → errorStatusCode
    // - IsPartiallySuccessful → partialStatusCode
    // - senao → successStatusCode
    //
    // Para casos especificos, basta override nos parametros opcionais:
    //   return Respond(output, ctx, successStatusCode: 201);   // Register → Created
    //   return Respond(output, ctx, errorStatusCode: 401);     // Login → Unauthorized
    protected IActionResult Respond<T>(
        T? data,
        ExecutionContext executionContext,
        int successStatusCode = 200,
        int errorStatusCode = 422,
        int partialStatusCode = 207)
    {
        var statusCode = ResolveStatusCode(data, executionContext, successStatusCode, errorStatusCode, partialStatusCode);
        var envelope = BuildEnvelope(data, executionContext, statusCode, errorStatusCode);

        return StatusCode(statusCode, envelope);
    }

    private static int ResolveStatusCode<T>(
        T? data,
        ExecutionContext executionContext,
        int successStatusCode,
        int errorStatusCode,
        int partialStatusCode)
    {
        if (data is null || (executionContext.IsFaulted && !executionContext.IsPartiallySuccessful))
            return errorStatusCode;

        if (executionContext.IsPartiallySuccessful)
            return partialStatusCode;

        return successStatusCode;
    }

    private static ApiResponse<T> BuildEnvelope<T>(
        T? data,
        ExecutionContext executionContext,
        int statusCode,
        int errorStatusCode)
    {
        if (statusCode == errorStatusCode)
            return ApiResponseFactory.CreateEmpty<T>(executionContext);

        return ApiResponseFactory.Create(data, executionContext);
    }
}
