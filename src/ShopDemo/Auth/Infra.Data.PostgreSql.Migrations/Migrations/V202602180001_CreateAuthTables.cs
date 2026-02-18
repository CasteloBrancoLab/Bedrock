using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations.Attributes;
using FluentMigrator;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Migrations.Migrations;

[Migration(202602180001)]
[SqlScript(
    "Up/V202602180001__create_auth_tables.sql",
    "Down/V202602180001__create_auth_tables.sql")]
public sealed class V202602180001_CreateAuthTables : SqlScriptMigrationBase;
