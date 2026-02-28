namespace ShopDemo.Auth.Application.UseCases.RegisterUser.Models;

public sealed record RegisterUserOutput(
    Guid UserId,
    string Email
);
