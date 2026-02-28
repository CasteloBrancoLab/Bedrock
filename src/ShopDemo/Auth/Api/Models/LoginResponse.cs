namespace ShopDemo.Auth.Api.Models;

public sealed record LoginResponse(
    Guid UserId,
    string Email
);
