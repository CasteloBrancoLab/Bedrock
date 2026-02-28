using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Microsoft.AspNetCore.Mvc;
using ShopDemo.Auth.Api.Models;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser.Interfaces;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser.Models;
using ShopDemo.Auth.Application.UseCases.RegisterUser.Interfaces;
using ShopDemo.Auth.Application.UseCases.RegisterUser.Models;

namespace ShopDemo.Auth.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private const string GenericErrorCode = "InvalidRequest";
    private const string GenericErrorMessage = "The request could not be processed.";

    private readonly IRegisterUserUseCase _registerUserUseCase;
    private readonly IAuthenticateUserUseCase _authenticateUserUseCase;

    public AuthController(
        IRegisterUserUseCase registerUserUseCase,
        IAuthenticateUserUseCase authenticateUserUseCase
    )
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

    private ExecutionContext CreateExecutionContext(string businessOperationCode)
    {
        var correlationId = Guid.TryParse(
            Request.Headers["X-Correlation-Id"].FirstOrDefault(),
            out var parsed)
            ? parsed
            : Guid.NewGuid();

        var tenantId = Guid.TryParse(
            Request.Headers["X-Tenant-Id"].FirstOrDefault(),
            out var tenantParsed)
            ? tenantParsed
            : Guid.Empty;

        var executionUser = User.Identity?.Name ?? "anonymous";

        return ExecutionContext.Create(
            correlationId: correlationId,
            tenantInfo: TenantInfo.Create(tenantId),
            executionUser: executionUser,
            executionOrigin: "Api",
            businessOperationCode: businessOperationCode,
            minimumMessageType: MessageType.Information,
            timeProvider: TimeProvider.System);
    }
}
