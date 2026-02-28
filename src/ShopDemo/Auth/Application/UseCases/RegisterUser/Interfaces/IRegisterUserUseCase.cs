using Bedrock.BuildingBlocks.Application.UseCases.Interfaces;
using ShopDemo.Auth.Application.UseCases.RegisterUser.Models;

namespace ShopDemo.Auth.Application.UseCases.RegisterUser.Interfaces;

public interface IRegisterUserUseCase
    : IUseCase<RegisterUserInput, RegisterUserOutput>;
