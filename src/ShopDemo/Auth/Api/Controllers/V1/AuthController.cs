using Asp.Versioning;
using Bedrock.BuildingBlocks.Web.ExecutionContexts;
using Bedrock.BuildingBlocks.Web.WebApi.Controllers;
using Bedrock.BuildingBlocks.Web.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using ShopDemo.Auth.Api.Models;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser.Interfaces;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser.Models;
using ShopDemo.Auth.Application.UseCases.RegisterUser.Interfaces;
using ShopDemo.Auth.Application.UseCases.RegisterUser.Models;

namespace ShopDemo.Auth.Api.Controllers.V1;

[ApiVersion(1)]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : BedrockApiControllerBase
{
    private const string GenericErrorCode = "InvalidRequest";
    private const string GenericErrorMessage = "The request could not be processed.";

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
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken
    )
    {
        var executionContext = CreateExecutionContext("AUTH_REGISTER_USER");

        var input = new RegisterUserInput(request.Email, request.Password);
        var output = await _registerUserUseCase.ExecuteAsync(executionContext, input, cancellationToken);

        if (output is null)
            return BadRequest(new ErrorResponse(GenericErrorCode, GenericErrorMessage));

        return Created(string.Empty, new RegisterResponse(output.UserId, output.Email));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        var executionContext = CreateExecutionContext("AUTH_AUTHENTICATE_USER");

        var input = new AuthenticateUserInput(request.Email, request.Password);
        var output = await _authenticateUserUseCase.ExecuteAsync(executionContext, input, cancellationToken);

        if (output is null)
            return Unauthorized(new ErrorResponse(GenericErrorCode, GenericErrorMessage));

        return Ok(new LoginResponse(output.UserId, output.Email));
    }
}
