namespace ShopDemo.Auth.Application.UseCases.RegisterUser.Models;

public sealed record RegisterUserInput(
    string Email,
    string Password
);
