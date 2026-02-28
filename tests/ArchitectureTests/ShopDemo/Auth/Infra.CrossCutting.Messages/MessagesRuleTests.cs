using Bedrock.ArchitectureTests.ShopDemo.Auth.Infra.CrossCutting.Messages.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.ShopDemo.Auth.Infra.CrossCutting.Messages;

[Collection("Arch")]
public sealed class MessagesRuleTests(ArchFixture fixture, ITestOutputHelper output)
    : MessagesRuleTestsBase<ArchFixture>(fixture, output);
