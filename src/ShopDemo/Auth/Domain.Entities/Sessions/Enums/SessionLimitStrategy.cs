namespace ShopDemo.Auth.Domain.Entities.Sessions.Enums;

public enum SessionLimitStrategy : byte
{
    RejectNew = 1,
    RevokeOldest = 2
}
