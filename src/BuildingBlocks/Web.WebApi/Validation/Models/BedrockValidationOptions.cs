using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bedrock.BuildingBlocks.Web.WebApi.Validation.Models;

// Configuracao fluente para a resposta de validacao do Bedrock.
//
// Substitui APENAS o InvalidModelStateResponseFactory do [ApiController]
// para retornar Problem Details RFC 9457 consistente com o exception handling.
//
// A validacao em si e feita pelas DataAnnotations nativas do ASP.NET
// ([Required], [MaxLength], etc.) nos request models. Este building block
// so customiza o FORMAT da resposta de erro.
//
// Uso tipico:
//   new BedrockValidationOptions()
//       .Configure((problem, context) => { problem.Extensions["custom"] = "value"; })
public sealed class BedrockValidationOptions
{
    internal Action<ValidationProblemDetails, HttpContext>? ConfigureProblemDetails { get; private set; }

    // Callback para enriquecer o ValidationProblemDetails gerado
    // antes de ser serializado na resposta.
    public BedrockValidationOptions Configure(
        Action<ValidationProblemDetails, HttpContext> configure)
    {
        ConfigureProblemDetails = configure;
        return this;
    }
}
