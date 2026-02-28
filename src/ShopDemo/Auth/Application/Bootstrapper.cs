using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser.Interfaces;
using ShopDemo.Auth.Application.UseCases.RegisterUser;
using ShopDemo.Auth.Application.UseCases.RegisterUser.Interfaces;

namespace ShopDemo.Auth.Application;

public static class Bootstrapper
{
    public static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.TryAddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
        services.TryAddScoped<IAuthenticateUserUseCase, AuthenticateUserUseCase>();

        return services;
    }
}
