namespace ShopDemo.Auth.Api.Models;

public sealed record RegisterResponse(
    Guid UserId,
    string Email
);
