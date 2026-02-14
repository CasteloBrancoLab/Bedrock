namespace ShopDemo.Auth.Domain.Entities.Users.Inputs;

public readonly record struct ChangePasswordHashInput(
    PasswordHash NewPasswordHash
);
