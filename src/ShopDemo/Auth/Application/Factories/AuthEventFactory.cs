using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Events;
using ShopDemo.Auth.Infra.CrossCutting.Messages.V1.Models;

namespace ShopDemo.Auth.Application.Factories;

public static class AuthEventFactory
{
    public static UserRegisteredEvent CreateUserRegistered(
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
