using Bedrock.ArchitectureTests.ShopDemo.Auth.Infra.Data.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.ShopDemo.Auth.Infra.Data;

[Collection("Arch")]
public sealed class InfrastructureRuleTests(ArchFixture fixture, ITestOutputHelper output)
    : InfrastructureRuleTestsBase<ArchFixture>(fixture, output);
