namespace ShopDemo.Auth.Domain.Entities.SigningKeys.Enums;

public enum SigningKeyStatus : byte
{
    Active = 1,
    Rotated = 2,
    Revoked = 3
}
