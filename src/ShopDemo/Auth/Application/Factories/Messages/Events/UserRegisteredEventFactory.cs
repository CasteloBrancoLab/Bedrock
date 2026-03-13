using ShopDemo.Auth.Application.Factories.Messages.Models;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

namespace ShopDemo.Auth.Application.Factories.Messages.Events;

public static class UserRegisteredEventFactory
{
    public static UserRegisteredEvent Create(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        string email,
        User user)
    {
        var metadata = AuthMessageMetadataFactory.Create(executionContext, timeProvider);
        var userModel = UserModelFactory.FromEntity(user, executionContext);

        return new UserRegisteredEvent(
            metadata,
            new RegisterUserInputModel(email),
            userModel);
    }
}
