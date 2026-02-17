using Bedrock.ArchitectureTests.ShopDemo.Auth.Infra.Data.PostgreSql.Migrations.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.ShopDemo.Auth.Infra.Data.PostgreSql.Migrations;

[Collection("Arch")]
public sealed class CodeStyleRuleTests(ArchFixture fixture, ITestOutputHelper output)
    : CodeStyleRuleTestsBase<ArchFixture>(fixture, output);
