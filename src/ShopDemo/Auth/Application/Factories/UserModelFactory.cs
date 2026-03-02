using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

namespace ShopDemo.Auth.Application.Factories;

public static class UserModelFactory
{
    public static UserModel FromEntity(User user, ExecutionContext executionContext)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(executionContext);

        return new UserModel(
            Id: user.EntityInfo.Id.Value,
            TenantCode: executionContext.TenantInfo.Code,
            Username: user.Username,
            Email: user.Email.Value ?? string.Empty,
            Status: user.Status.ToString(),
            CreatedAt: user.EntityInfo.EntityChangeInfo.CreatedAt,
            CreatedBy: user.EntityInfo.EntityChangeInfo.CreatedBy,
            LastChangedAt: user.EntityInfo.EntityChangeInfo.LastChangedAt,
            LastChangedBy: user.EntityInfo.EntityChangeInfo.LastChangedBy);
    }
}
