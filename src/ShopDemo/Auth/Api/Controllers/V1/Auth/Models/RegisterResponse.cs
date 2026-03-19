namespace ShopDemo.Auth.Api.Controllers.V1.Auth.Models;

public sealed record RegisterResponse(
    Guid UserId,
    string Email
);
