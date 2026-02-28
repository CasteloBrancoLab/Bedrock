namespace ShopDemo.Auth.Application.UseCases.AuthenticateUser.Models;

public sealed record AuthenticateUserOutput(
    Guid UserId,
    string Email
);
