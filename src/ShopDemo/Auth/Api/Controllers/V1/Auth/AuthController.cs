using Asp.Versioning;
using Bedrock.BuildingBlocks.Web.ExecutionContexts;
using Bedrock.BuildingBlocks.Web.ExecutionContexts.Interfaces;
using Bedrock.BuildingBlocks.Web.WebApi.Controllers;
using Bedrock.BuildingBlocks.Web.WebApi.RateLimiting.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShopDemo.Auth.Api.Constants;
using ShopDemo.Auth.Api.Controllers.V1.Auth.Models;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser.Interfaces;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser.Models;
using ShopDemo.Auth.Application.UseCases.RegisterUser.Interfaces;
using ShopDemo.Auth.Application.UseCases.RegisterUser.Models;

namespace ShopDemo.Auth.Api.Controllers.V1.Auth;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/auth")]
[EnableRateLimiting(BedrockRateLimitingPolicyNames.Tenant)]
public sealed class AuthController : BedrockApiControllerBase
{
    private readonly IRegisterUserUseCase _registerUserUseCase;
    private readonly IAuthenticateUserUseCase _authenticateUserUseCase;

    public AuthController(
        IExecutionContextFactory executionContextFactory,
        IRegisterUserUseCase registerUserUseCase,
        IAuthenticateUserUseCase authenticateUserUseCase
    ) : base(executionContextFactory)
    {
        _registerUserUseCase = registerUserUseCase ?? throw new ArgumentNullException(nameof(registerUserUseCase));
        _authenticateUserUseCase = authenticateUserUseCase ?? throw new ArgumentNullException(nameof(authenticateUserUseCase));
    }

    [HttpPost("register")]
    [EnableRateLimiting(RateLimitPolicyNames.Register)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterPayload payload,
        CancellationToken cancellationToken
    )
    {
        var executionContext = CreateExecutionContext();

        var input = new RegisterUserInput(payload.Email, payload.Password);
        var output = await _registerUserUseCase.ExecuteAsync(executionContext, input, cancellationToken);

        var response = output is not null
            ? new RegisterResponse(output.UserId, output.Email)
            : (RegisterResponse?)null;

        return Respond(response, executionContext, successStatusCode: 201);
    }

    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicyNames.Login)]
    public async Task<IActionResult> Login(
        [FromBody] LoginPayload payload,
        CancellationToken cancellationToken
    )
    {
        var executionContext = CreateExecutionContext();

        var input = new AuthenticateUserInput(payload.Email, payload.Password);
        var output = await _authenticateUserUseCase.ExecuteAsync(executionContext, input, cancellationToken);

        var response = output is not null
            ? new LoginResponse(output.UserId, output.Email)
            : (LoginResponse?)null;

        return Respond(response, executionContext, errorStatusCode: 401);
    }
}
