using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations.Attributes;
using FluentMigrator;

namespace Bedrock.IntegrationTests.BuildingBlocks.Persistence.PostgreSql.Migrations.TestMigrations;

[Migration(202602140002)]
[SqlScript(
    "Up/V202602140002__add_test_column.sql",
    "Down/V202602140002__add_test_column.sql")]
public sealed class V202602140002_AddTestColumn : SqlScriptMigrationBase;
