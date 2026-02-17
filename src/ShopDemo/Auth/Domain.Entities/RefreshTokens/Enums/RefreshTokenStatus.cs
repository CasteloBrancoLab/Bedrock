namespace ShopDemo.Auth.Domain.Entities.RefreshTokens.Enums;

public enum RefreshTokenStatus : byte
{
    Active = 1,
    Used = 2,
    Revoked = 3
}
