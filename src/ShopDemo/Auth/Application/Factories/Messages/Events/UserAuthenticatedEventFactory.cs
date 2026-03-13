using ShopDemo.Auth.Application.Factories.Messages.Models;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

namespace ShopDemo.Auth.Application.Factories.Messages.Events;

public static class UserAuthenticatedEventFactory
{
    public static UserAuthenticatedEvent Create(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        string email,
        User user)
    {
        var metadata = AuthMessageMetadataFactory.Create(executionContext, timeProvider);
        var userModel = UserModelFactory.FromEntity(user, executionContext);

        return new UserAuthenticatedEvent(
            metadata,
            new AuthenticateUserInputModel(email),
            userModel);
    }
}
