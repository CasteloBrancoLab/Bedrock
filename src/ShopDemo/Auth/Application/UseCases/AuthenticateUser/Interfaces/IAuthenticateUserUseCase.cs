using Bedrock.BuildingBlocks.Application.UseCases.Interfaces;
using ShopDemo.Auth.Application.UseCases.AuthenticateUser.Models;

namespace ShopDemo.Auth.Application.UseCases.AuthenticateUser.Interfaces;

public interface IAuthenticateUserUseCase
    : IUseCase<AuthenticateUserInput, AuthenticateUserOutput>;
