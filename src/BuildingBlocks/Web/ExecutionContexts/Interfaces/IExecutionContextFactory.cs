using Bedrock.BuildingBlocks.Core.ExecutionContexts;
using Microsoft.AspNetCore.Http;

namespace Bedrock.BuildingBlocks.Web.ExecutionContexts.Interfaces;

public interface IExecutionContextFactory
{
    ExecutionContext Create(HttpContext httpContext);
}
