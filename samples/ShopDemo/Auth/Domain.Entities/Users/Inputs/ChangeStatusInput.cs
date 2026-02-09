using ShopDemo.Auth.Domain.Entities.Users.Enums;

namespace ShopDemo.Auth.Domain.Entities.Users.Inputs;

public readonly record struct ChangeStatusInput(
    UserStatus NewStatus
);
