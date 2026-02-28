namespace ShopDemo.Auth.Application.UseCases.AuthenticateUser.Models;

public sealed record AuthenticateUserInput(
    string Email,
    string Password
);
