using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bedrock.BuildingBlocks.Web.WebApi.ExceptionHandling.Models;

// Configuracao fluente para o exception handling do Bedrock.
//
// Define mapeamentos de exception para HTTP status code e permite
// enriquecer o ProblemDetails gerado via callback.
//
// Defaults:
// - ArgumentException → 400
// - KeyNotFoundException → 404
// - UnauthorizedAccessException → 403
// - InvalidOperationException → 422
// - OperationCanceledException → 499
// - Tudo mais → 500
//
// Uso tipico:
//   new BedrockExceptionHandlingOptions()
//       .MapException<BusinessRuleException>(StatusCodes.Status422UnprocessableEntity)
//       .WithExceptionDetails()
//       .Configure((problem, context, exception) => { ... })
public sealed class BedrockExceptionHandlingOptions
{
    internal Dictionary<Type, int> ExceptionStatusCodeMap { get; } = new()
    {
        [typeof(ArgumentException)] = StatusCodes.Status400BadRequest,
        [typeof(ArgumentNullException)] = StatusCodes.Status400BadRequest,
        [typeof(KeyNotFoundException)] = StatusCodes.Status404NotFound,
        [typeof(UnauthorizedAccessException)] = StatusCodes.Status403Forbidden,
        [typeof(InvalidOperationException)] = StatusCodes.Status422UnprocessableEntity,
        [typeof(OperationCanceledException)] = StatusCodes.Status499ClientClosedRequest,
    };

    internal bool IncludeExceptionDetails { get; private set; }
    internal Action<ProblemDetails, HttpContext, Exception>? ConfigureProblemDetails { get; private set; }

    // Mapeia um tipo de exception para um HTTP status code especifico.
    // Sobrescreve mapeamentos default se o tipo ja existir.
    public BedrockExceptionHandlingOptions MapException<TException>(int statusCode)
        where TException : Exception
    {
        ExceptionStatusCodeMap[typeof(TException)] = statusCode;
        return this;
    }

    // Inclui detalhes da exception (message, stack trace) no ProblemDetails.
    // Deve ser usado APENAS em ambiente de desenvolvimento.
    public BedrockExceptionHandlingOptions WithExceptionDetails()
    {
        IncludeExceptionDetails = true;
        return this;
    }

    // Callback para enriquecer o ProblemDetails gerado antes de ser serializado.
    // Permite adicionar extensions customizadas, alterar titulo, etc.
    public BedrockExceptionHandlingOptions Configure(
        Action<ProblemDetails, HttpContext, Exception> configure)
    {
        ConfigureProblemDetails = configure;
        return this;
    }
}
