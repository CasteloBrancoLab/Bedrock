using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations.Attributes;
using FluentMigrator;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Migrations.Migrations;

[Migration(202602160001)]
[SqlScript(
    "Up/V202602160001__create_auth_users_table.sql",
    "Down/V202602160001__create_auth_users_table.sql")]
public sealed class V202602160001_CreateAuthUsersTable : SqlScriptMigrationBase;
