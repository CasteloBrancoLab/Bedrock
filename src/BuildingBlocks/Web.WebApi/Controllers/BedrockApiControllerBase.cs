using Bedrock.BuildingBlocks.Web.ExecutionContexts;
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
}
