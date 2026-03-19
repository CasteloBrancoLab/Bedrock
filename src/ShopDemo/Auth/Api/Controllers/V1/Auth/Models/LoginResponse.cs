namespace ShopDemo.Auth.Api.Controllers.V1.Auth.Models;

public sealed record LoginResponse(
    Guid UserId,
    string Email
);
