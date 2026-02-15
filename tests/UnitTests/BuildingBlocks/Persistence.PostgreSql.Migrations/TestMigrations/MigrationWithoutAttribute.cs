using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations;
using FluentMigrator;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Migrations.TestMigrations;

[Migration(202602140003)]
public sealed class MigrationWithoutAttribute : SqlScriptMigrationBase;
