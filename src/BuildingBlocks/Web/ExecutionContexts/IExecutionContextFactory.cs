using Microsoft.AspNetCore.Http;

namespace Bedrock.BuildingBlocks.Web.ExecutionContexts;

public interface IExecutionContextFactory
{
    ExecutionContext Create(HttpContext httpContext, string businessOperationCode);
}
