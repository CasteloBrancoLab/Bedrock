using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Bedrock.BuildingBlocks.Testing;
using Npgsql;
using NpgsqlTypes;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using SortDirection = Bedrock.BuildingBlocks.Core.Sortings.Enums.SortDirection;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Mappers;

public class DataModelMapperBaseTests : TestBase
{
    private readonly TestMapper _mapper;

    public DataModelMapperBaseTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
        _mapper = new TestMapper();
    }

    [Fact]
    public void Constructor_ShouldConfigureMapper()
    {
        // Arrange & Act
        LogArrange("Creating new mapper instance");
        TestMapper mapper = new();

        // Assert
        LogAssert("Verifying mapper is configured");
        mapper.TableSchema.ShouldBe("public");
        mapper.TableName.ShouldBe("test_entities");
    }

    [Fact]
    public void GetTableName_ShouldReturnFullyQualifiedTableName()
    {
        // Act
        LogAct("Getting table name");
        string tableName = _mapper.GetTableName();

        // Assert
        LogAssert("Verifying fully qualified table name");
        tableName.ShouldBe("public.test_entities");
    }

    [Fact]
    public void GetTableName_WithoutSchema_ShouldReturnTableNameOnly()
    {
        // Arrange
        LogArrange("Creating mapper without schema");
        TestMapperNoSchema mapper = new();

        // Act
        LogAct("Getting table name");
        string tableName = mapper.GetTableName();

        // Assert
        LogAssert("Verifying table name without schema");
        tableName.ShouldBe("test_entities");
    }

    [Fact]
    public void ColumnMapDictionary_ShouldContainBaseAndCustomColumns()
    {
        // Act
        LogAct("Getting column map dictionary");
        var columns = _mapper.ColumnMapDictionary;

        // Assert
        LogAssert("Verifying columns are mapped");
        columns.ShouldContainKey("Id");
        columns.ShouldContainKey("TenantCode");
        columns.ShouldContainKey("CreatedBy");
        columns.ShouldContainKey("CustomField");
    }

    [Fact]
    public void GetColumnMap_WithValidPropertyName_ShouldReturnColumnMap()
    {
        // Act
        LogAct("Getting column map for Id property");
        ColumnMap columnMap = _mapper.GetColumnMap("Id");

        // Assert
        LogAssert("Verifying column map is returned");
        columnMap.ColumnName.ShouldBe("id");
        columnMap.Type.ShouldBe(typeof(Guid));
        columnMap.NpgsqlDbType.ShouldBe(NpgsqlDbType.Uuid);
    }

    [Fact]
    public void GetColumnMap_WithInvalidPropertyName_ShouldThrow()
    {
        // Act & Assert
        LogAct("Getting column map for invalid property");
        var exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            _mapper.GetColumnMap("InvalidProperty"));
        exception.ParamName.ShouldBe("propertyName");
    }

    [Fact]
    public void GetColumnMap_WithExpression_ShouldReturnColumnMap()
    {
        // Act
        LogAct("Getting column map using expression");
        ColumnMap columnMap = _mapper.GetColumnMap(x => x.CustomField);

        // Assert
        LogAssert("Verifying column map is returned");
        columnMap.ColumnName.ShouldBe("custom_field");
    }

    [Fact]
    public void GetColumnName_ShouldReturnColumnName()
    {
        // Act
        LogAct("Getting column name");
        string columnName = _mapper.GetColumnName(x => x.CustomField);

        // Assert
        LogAssert("Verifying column name");
        columnName.ShouldBe("custom_field");
    }

    [Fact]
    public void GetColumnType_ShouldReturnColumnType()
    {
        // Act
        LogAct("Getting column type");
        Type columnType = _mapper.GetColumnType(x => x.CustomField);

        // Assert
        LogAssert("Verifying column type");
        columnType.ShouldBe(typeof(string));
    }

    [Fact]
    public void GetColumnNpgsqlDbType_ShouldReturnNpgsqlDbType()
    {
        // Act
        LogAct("Getting NpgsqlDbType");
        NpgsqlDbType dbType = _mapper.GetColumnNpgsqlDbType(x => x.CustomField);

        // Assert
        LogAssert("Verifying NpgsqlDbType");
        dbType.ShouldBe(NpgsqlDbType.Varchar);
    }

    [Fact]
    public void SelectCommand_ShouldBeGenerated()
    {
        // Act
        LogAct("Getting select command");
        string selectCommand = _mapper.SelectCommand;

        // Assert
        LogAssert("Verifying select command structure");
        selectCommand.ShouldStartWith("SELECT ");
        selectCommand.ShouldContain("FROM public.test_entities");
        selectCommand.ShouldContain("WHERE ");
    }

    [Fact]
    public void InsertCommand_ShouldBeGenerated()
    {
        // Act
        LogAct("Getting insert command");
        string insertCommand = _mapper.InsertCommand;

        // Assert
        LogAssert("Verifying insert command structure");
        insertCommand.ShouldStartWith("INSERT INTO public.test_entities");
        insertCommand.ShouldContain("VALUES");
    }

    [Fact]
    public void UpdateCommand_ShouldBeGenerated()
    {
        // Act
        LogAct("Getting update command");
        string updateCommand = _mapper.UpdateCommand;

        // Assert
        LogAssert("Verifying update command structure");
        updateCommand.ShouldStartWith("UPDATE public.test_entities SET");
        updateCommand.ShouldContain("WHERE ");
    }

    [Fact]
    public void DeleteCommand_ShouldBeGenerated()
    {
        // Act
        LogAct("Getting delete command");
        string deleteCommand = _mapper.DeleteCommand;

        // Assert
        LogAssert("Verifying delete command structure");
        deleteCommand.ShouldStartWith("DELETE FROM public.test_entities");
        deleteCommand.ShouldContain("WHERE ");
    }

    [Fact]
    public void CopyCommand_ShouldBeGenerated()
    {
        // Act
        LogAct("Getting copy command");
        string copyCommand = _mapper.CopyCommand;

        // Assert
        LogAssert("Verifying copy command structure");
        copyCommand.ShouldStartWith("COPY public.test_entities");
        copyCommand.ShouldContain("FROM STDIN");
        copyCommand.ShouldContain("FORMAT BINARY");
    }

    [Fact]
    public void GenerateSelectCommand_WithWhereClause_ShouldAppendClause()
    {
        // Arrange
        LogArrange("Creating where clause");
        WhereClause whereClause = _mapper.Where(x => x.CustomField);

        // Act
        LogAct("Generating select command with where clause");
        string command = _mapper.GenerateSelectCommand(whereClause);

        // Assert
        LogAssert("Verifying where clause is appended");
        command.ShouldContain("AND (public.test_entities.custom_field = @public_test_entities_CustomField)");
    }

    [Fact]
    public void GenerateSelectCommand_WithWhereAndPagination_ShouldIncludeLimitOffset()
    {
        // Arrange
        LogArrange("Creating where clause and pagination");
        WhereClause whereClause = _mapper.Where(x => x.CustomField);
        PaginationInfo pagination = PaginationInfo.Create(page: 2, pageSize: 10);

        // Act
        LogAct("Generating select command with where and pagination");
        string command = _mapper.GenerateSelectCommand(whereClause, pagination);

        // Assert
        LogAssert("Verifying LIMIT and OFFSET are present");
        command.ShouldContain("LIMIT 10");
        command.ShouldContain("OFFSET 10");
    }

    [Fact]
    public void GenerateSelectCommand_WithPagination_ShouldIncludeLimitOffset()
    {
        // Arrange
        LogArrange("Creating pagination");
        PaginationInfo pagination = PaginationInfo.Create(page: 1, pageSize: 25);

        // Act
        LogAct("Generating select command with pagination");
        string command = _mapper.GenerateSelectCommand(pagination);

        // Assert
        LogAssert("Verifying LIMIT and OFFSET are present");
        command.ShouldContain("LIMIT 25");
        command.ShouldContain("OFFSET 0");
    }

    [Fact]
    public void GenerateSelectCommand_WithWhereAndOrderBy_ShouldIncludeOrderBy()
    {
        // Arrange
        LogArrange("Creating where and order by clauses");
        WhereClause whereClause = _mapper.Where(x => x.CustomField);
        OrderByClause orderBy = _mapper.OrderByAscending(x => x.CustomField);

        // Act
        LogAct("Generating select command with where and order by");
        string command = _mapper.GenerateSelectCommand(whereClause, orderBy);

        // Assert
        LogAssert("Verifying ORDER BY is present");
        command.ShouldContain("ORDER BY public.test_entities.custom_field ASC");
    }

    [Fact]
    public void GenerateSelectCommand_WithWhereOrderByAndPagination_ShouldIncludeAll()
    {
        // Arrange
        LogArrange("Creating where, order by, and pagination");
        WhereClause whereClause = _mapper.Where(x => x.CustomField);
        OrderByClause orderBy = _mapper.OrderByDescending(x => x.CustomField);
        PaginationInfo pagination = PaginationInfo.Create(page: 3, pageSize: 20);

        // Act
        LogAct("Generating select command with all clauses");
        string command = _mapper.GenerateSelectCommand(whereClause, orderBy, pagination);

        // Assert
        LogAssert("Verifying all clauses are present");
        command.ShouldContain("ORDER BY public.test_entities.custom_field DESC");
        command.ShouldContain("LIMIT 20");
        command.ShouldContain("OFFSET 40");
    }

    [Fact]
    public void GenerateSelectCommand_WithOrderByOnly_ShouldIncludeOrderBy()
    {
        // Arrange
        LogArrange("Creating order by clause");
        OrderByClause orderBy = _mapper.OrderByAscending(x => x.CustomField);

        // Act
        LogAct("Generating select command with order by");
        string command = _mapper.GenerateSelectCommand(orderBy);

        // Assert
        LogAssert("Verifying ORDER BY is present");
        command.ShouldContain("ORDER BY public.test_entities.custom_field ASC");
    }

    [Fact]
    public void GenerateSelectCommand_WithOrderByAndPagination_ShouldIncludeBoth()
    {
        // Arrange
        LogArrange("Creating order by and pagination");
        OrderByClause orderBy = _mapper.OrderByDescending(x => x.CustomField);
        PaginationInfo pagination = PaginationInfo.Create(page: 1, pageSize: 15);

        // Act
        LogAct("Generating select command with order by and pagination");
        string command = _mapper.GenerateSelectCommand(orderBy, pagination);

        // Assert
        LogAssert("Verifying both clauses are present");
        command.ShouldContain("ORDER BY public.test_entities.custom_field DESC");
        command.ShouldContain("LIMIT 15");
    }

    [Fact]
    public void GenerateUpdateCommand_WithWhereClause_ShouldAppendClause()
    {
        // Arrange
        LogArrange("Creating where clause");
        WhereClause whereClause = _mapper.Where(x => x.CustomField);

        // Act
        LogAct("Generating update command with where clause");
        string command = _mapper.GenerateUpdateCommand(whereClause);

        // Assert
        LogAssert("Verifying where clause is appended");
        command.ShouldStartWith("UPDATE public.test_entities SET");
        command.ShouldContain("AND (public.test_entities.custom_field = @public_test_entities_CustomField)");
    }

    [Fact]
    public void GenerateDeleteCommand_WithWhereClause_ShouldAppendClause()
    {
        // Arrange
        LogArrange("Creating where clause");
        WhereClause whereClause = _mapper.Where(x => x.CustomField);

        // Act
        LogAct("Generating delete command with where clause");
        string command = _mapper.GenerateDeleteCommand(whereClause);

        // Assert
        LogAssert("Verifying where clause is appended");
        command.ShouldStartWith("DELETE FROM public.test_entities");
        command.ShouldContain("AND (public.test_entities.custom_field = @public_test_entities_CustomField)");
    }

    [Fact]
    public void GenerateExistsCommand_WithWhereClause_ShouldGenerateExistsQuery()
    {
        // Arrange
        LogArrange("Creating where clause");
        WhereClause whereClause = _mapper.Where(x => x.CustomField);

        // Act
        LogAct("Generating exists command");
        string command = _mapper.GenerateExistsCommand(whereClause);

        // Assert
        LogAssert("Verifying EXISTS query structure");
        command.ShouldStartWith("SELECT EXISTS");
        command.ShouldContain("SELECT 1 FROM public.test_entities");
    }

    [Fact]
    public void Where_WithPropertyName_ShouldReturnWhereClause()
    {
        // Act
        LogAct("Creating where clause by property name");
        WhereClause whereClause = _mapper.Where("CustomField");

        // Assert
        LogAssert("Verifying where clause");
        whereClause.ToString().ShouldContain("custom_field = ");
    }

    [Fact]
    public void Where_WithPropertyNameAndOperator_ShouldReturnWhereClause()
    {
        // Act
        LogAct("Creating where clause with operator");
        WhereClause whereClause = _mapper.Where("CustomField", RelationalOperator.GreaterThan);

        // Assert
        LogAssert("Verifying where clause with operator");
        whereClause.ToString().ShouldContain("custom_field > ");
    }

    [Fact]
    public void Where_WithExpression_ShouldReturnWhereClause()
    {
        // Act
        LogAct("Creating where clause with expression");
        WhereClause whereClause = _mapper.Where(x => x.CustomField);

        // Assert
        LogAssert("Verifying where clause");
        whereClause.ToString().ShouldContain("custom_field = ");
    }

    [Fact]
    public void Where_WithExpressionAndOperator_ShouldReturnWhereClause()
    {
        // Act
        LogAct("Creating where clause with expression and operator");
        WhereClause whereClause = _mapper.Where(x => x.CustomField, RelationalOperator.NotEqual);

        // Assert
        LogAssert("Verifying where clause with operator");
        whereClause.ToString().ShouldContain("custom_field <> ");
    }

    [Fact]
    public void WhereWithParameterSuffix_WithPropertyName_ShouldReturnWhereClauseWithSuffix()
    {
        // Act
        LogAct("Creating where clause with parameter suffix");
        WhereClause whereClause = _mapper.WhereWithParameterSuffix("CustomField", "_expected", RelationalOperator.Equal);

        // Assert
        LogAssert("Verifying where clause contains suffix");
        string clause = whereClause.ToString();
        clause.ShouldContain("custom_field =");
        clause.ShouldContain("_expected");
    }

    [Fact]
    public void WhereWithParameterSuffix_WithExpression_ShouldReturnWhereClauseWithSuffix()
    {
        // Act
        LogAct("Creating where clause with expression and parameter suffix");
        WhereClause whereClause = _mapper.WhereWithParameterSuffix(x => x.CustomField, "_version", RelationalOperator.GreaterThan);

        // Assert
        LogAssert("Verifying where clause contains suffix and operator");
        string clause = whereClause.ToString();
        clause.ShouldContain("custom_field >");
        clause.ShouldContain("_version");
    }

    [Fact]
    public void WhereWithParameterSuffix_WithDifferentOperators_ShouldReturnCorrectClause()
    {
        // Act
        LogAct("Creating where clauses with different operators");
        WhereClause lessThan = _mapper.WhereWithParameterSuffix("CustomField", "_lt", RelationalOperator.LessThan);
        WhereClause greaterOrEqual = _mapper.WhereWithParameterSuffix("CustomField", "_gte", RelationalOperator.GreaterThanOrEqual);
        WhereClause notEqual = _mapper.WhereWithParameterSuffix("CustomField", "_ne", RelationalOperator.NotEqual);

        // Assert
        LogAssert("Verifying each operator is correctly applied");
        lessThan.ToString().ShouldContain("< @");
        lessThan.ToString().ShouldContain("_lt");
        greaterOrEqual.ToString().ShouldContain(">= @");
        greaterOrEqual.ToString().ShouldContain("_gte");
        notEqual.ToString().ShouldContain("<> @");
        notEqual.ToString().ShouldContain("_ne");
    }

    [Fact]
    public void WhereWithParameterSuffix_ShouldIncludeTableNameInClause()
    {
        // Act
        LogAct("Creating where clause with suffix");
        WhereClause whereClause = _mapper.WhereWithParameterSuffix(x => x.CustomField, "_test", RelationalOperator.Equal);

        // Assert
        LogAssert("Verifying table name is included in clause");
        string clause = whereClause.ToString();
        clause.ShouldContain("public.test_entities.custom_field");
    }

    [Fact]
    public void OrderBy_WithPropertyNameAndAscending_ShouldReturnOrderByClause()
    {
        // Act
        LogAct("Creating order by clause ascending");
        OrderByClause orderBy = _mapper.OrderBy("CustomField", SortDirection.Ascending);

        // Assert
        LogAssert("Verifying order by clause");
        orderBy.ToString().ShouldContain("ASC");
    }

    [Fact]
    public void OrderBy_WithPropertyNameAndDescending_ShouldReturnOrderByClause()
    {
        // Act
        LogAct("Creating order by clause descending");
        OrderByClause orderBy = _mapper.OrderBy("CustomField", SortDirection.Descending);

        // Assert
        LogAssert("Verifying order by clause");
        orderBy.ToString().ShouldContain("DESC");
    }

    [Fact]
    public void OrderBy_WithExpression_ShouldReturnOrderByClause()
    {
        // Act
        LogAct("Creating order by clause with expression");
        OrderByClause orderBy = _mapper.OrderBy(x => x.CustomField, SortDirection.Ascending);

        // Assert
        LogAssert("Verifying order by clause");
        orderBy.ToString().ShouldContain("custom_field ASC");
    }

    [Fact]
    public void OrderByAscending_WithPropertyName_ShouldReturnAscendingClause()
    {
        // Act
        LogAct("Creating ascending order by clause");
        OrderByClause orderBy = _mapper.OrderByAscending("CustomField");

        // Assert
        LogAssert("Verifying ascending order by clause");
        orderBy.ToString().ShouldContain("ASC");
    }

    [Fact]
    public void OrderByAscending_WithExpression_ShouldReturnAscendingClause()
    {
        // Act
        LogAct("Creating ascending order by clause with expression");
        OrderByClause orderBy = _mapper.OrderByAscending(x => x.CustomField);

        // Assert
        LogAssert("Verifying ascending order by clause");
        orderBy.ToString().ShouldContain("ASC");
    }

    [Fact]
    public void OrderByDescending_WithPropertyName_ShouldReturnDescendingClause()
    {
        // Act
        LogAct("Creating descending order by clause");
        OrderByClause orderBy = _mapper.OrderByDescending("CustomField");

        // Assert
        LogAssert("Verifying descending order by clause");
        orderBy.ToString().ShouldContain("DESC");
    }

    [Fact]
    public void OrderByDescending_WithExpression_ShouldReturnDescendingClause()
    {
        // Act
        LogAct("Creating descending order by clause with expression");
        OrderByClause orderBy = _mapper.OrderByDescending(x => x.CustomField);

        // Assert
        LogAssert("Verifying descending order by clause");
        orderBy.ToString().ShouldContain("DESC");
    }

    [Fact]
    public void GetParameterName_WithPropertyName_ShouldReturnFormattedParameterName()
    {
        // Act
        LogAct("Getting parameter name");
        string parameterName = _mapper.GetParameterName("CustomField");

        // Assert
        LogAssert("Verifying parameter name format");
        parameterName.ShouldBe("@public_test_entities_CustomField");
    }

    [Fact]
    public void GetParameterName_WithExpression_ShouldReturnFormattedParameterName()
    {
        // Act
        LogAct("Getting parameter name with expression");
        string parameterName = _mapper.GetParameterName(x => x.CustomField);

        // Assert
        LogAssert("Verifying parameter name format");
        parameterName.ShouldBe("@public_test_entities_CustomField");
    }

    [Fact]
    public void GetParameterName_WithoutSchema_ShouldNotIncludeSchema()
    {
        // Arrange
        LogArrange("Creating mapper without schema");
        TestMapperNoSchema mapper = new();

        // Act
        LogAct("Getting parameter name");
        string parameterName = mapper.GetParameterName("CustomField");

        // Assert
        LogAssert("Verifying parameter name without schema");
        parameterName.ShouldBe("@test_entities_CustomField");
    }

    [Fact]
    public void GetColumnNamesWithAlias_ShouldReturnAliasedColumnNames()
    {
        // Act
        LogAct("Getting column names with alias");
        string columnNames = _mapper.GetColumnNamesWithAlias();

        // Assert
        LogAssert("Verifying aliased column names");
        columnNames.ShouldContain("public.test_entities.id AS \"test_entities_Id\"");
        columnNames.ShouldContain("public.test_entities.custom_field AS \"test_entities_CustomField\"");
    }

    [Fact]
    public void ColumnMapDictionary_ShouldBeCachedOnSecondAccess()
    {
        // Act
        LogAct("Accessing ColumnMapDictionary twice");
        var first = _mapper.ColumnMapDictionary;
        var second = _mapper.ColumnMapDictionary;

        // Assert
        LogAssert("Verifying same instance is returned");
        first.ShouldBeSameAs(second);
    }

    [Fact]
    public void Configure_ShouldMapDataModelBaseColumns()
    {
        // Act
        LogAct("Verifying DataModelBase columns are mapped");
        var columns = _mapper.ColumnMapDictionary;

        // Assert
        LogAssert("Verifying all DataModelBase columns are present");
        columns.ShouldContainKey("Id");
        columns["Id"].ColumnName.ShouldBe("id");
        columns["Id"].Type.ShouldBe(typeof(Guid));

        columns.ShouldContainKey("TenantCode");
        columns["TenantCode"].ColumnName.ShouldBe("tenant_code");
        columns["TenantCode"].Type.ShouldBe(typeof(Guid));

        columns.ShouldContainKey("CreatedBy");
        columns["CreatedBy"].ColumnName.ShouldBe("created_by");
        columns["CreatedBy"].Type.ShouldBe(typeof(string));

        columns.ShouldContainKey("CreatedAt");
        columns["CreatedAt"].ColumnName.ShouldBe("created_at");
        columns["CreatedAt"].Type.ShouldBe(typeof(DateTimeOffset));

        columns.ShouldContainKey("LastChangedBy");
        columns["LastChangedBy"].ColumnName.ShouldBe("last_changed_by");
        columns["LastChangedBy"].Type.ShouldBe(typeof(string));

        columns.ShouldContainKey("LastChangedAt");
        columns["LastChangedAt"].ColumnName.ShouldBe("last_changed_at");
        columns["LastChangedAt"].Type.ShouldBe(typeof(DateTimeOffset?));

        columns.ShouldContainKey("LastChangedExecutionOrigin");
        columns["LastChangedExecutionOrigin"].ColumnName.ShouldBe("last_changed_execution_origin");
        columns["LastChangedExecutionOrigin"].Type.ShouldBe(typeof(string));

        columns.ShouldContainKey("LastChangedCorrelationId");
        columns["LastChangedCorrelationId"].ColumnName.ShouldBe("last_changed_correlation_id");
        columns["LastChangedCorrelationId"].Type.ShouldBe(typeof(Guid?));

        columns.ShouldContainKey("LastChangedBusinessOperationCode");
        columns["LastChangedBusinessOperationCode"].ColumnName.ShouldBe("last_changed_business_operation_code");
        columns["LastChangedBusinessOperationCode"].Type.ShouldBe(typeof(string));

        columns.ShouldContainKey("EntityVersion");
        columns["EntityVersion"].ColumnName.ShouldBe("entity_version");
        columns["EntityVersion"].Type.ShouldBe(typeof(long));
    }

    [Fact]
    public void UpdateCommand_ShouldContainTenantAndIdInWhereClause()
    {
        // Act
        LogAct("Getting update command");
        string updateCommand = _mapper.UpdateCommand;

        // Assert
        LogAssert("Verifying update command contains tenant and id in WHERE clause");
        updateCommand.ShouldContain("tenant_code =");
        updateCommand.ShouldContain("id =");
    }

    [Fact]
    public void Configure_WithNullTableName_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        LogAct("Creating mapper with null table name");
        Should.Throw<ArgumentNullException>(() => new TestMapperNullTableName());
    }

    [Fact]
    public void Where_CalledTwice_ShouldReturnCachedResult()
    {
        // Arrange
        LogArrange("Creating mapper instance");

        // Act
        LogAct("Calling Where twice with same parameters");
        WhereClause first = _mapper.Where("CustomField", RelationalOperator.Equal);
        WhereClause second = _mapper.Where("CustomField", RelationalOperator.Equal);

        // Assert
        LogAssert("Verifying cached result is returned");
        first.Value.ShouldBe(second.Value);
    }

    [Fact]
    public void OrderBy_CalledTwice_ShouldReturnCachedResult()
    {
        // Arrange
        LogArrange("Creating mapper instance");

        // Act
        LogAct("Calling OrderBy twice with same parameters");
        OrderByClause first = _mapper.OrderBy("CustomField", SortDirection.Ascending);
        OrderByClause second = _mapper.OrderBy("CustomField", SortDirection.Ascending);

        // Assert
        LogAssert("Verifying cached result is returned");
        first.Value.ShouldBe(second.Value);
    }
}

/// <summary>
/// Test data model for mapper tests
/// </summary>
public class TestEntityDataModel : DataModelBase
{
    public string CustomField { get; set; } = null!;
}

/// <summary>
/// Test mapper implementation
/// </summary>
internal sealed class TestMapper : DataModelMapperBase<TestEntityDataModel>
{
    protected override void ConfigureInternal(MapperOptions<TestEntityDataModel> mapperOptions)
    {
        mapperOptions
            .MapTable("public", "test_entities")
            .MapColumn(x => x.CustomField);
    }

    public override void MapBinaryImporter(NpgsqlBinaryImporter importer, TestEntityDataModel model)
    {
        // Not implemented for tests
    }
}

/// <summary>
/// Test mapper without schema
/// </summary>
internal sealed class TestMapperNoSchema : DataModelMapperBase<TestEntityDataModel>
{
    protected override void ConfigureInternal(MapperOptions<TestEntityDataModel> mapperOptions)
    {
        mapperOptions
            .MapTable(null, "test_entities")
            .MapColumn(x => x.CustomField);
    }

    public override void MapBinaryImporter(NpgsqlBinaryImporter importer, TestEntityDataModel model)
    {
        // Not implemented for tests
    }
}

/// <summary>
/// Test mapper with null table name to test ArgumentNullException
/// </summary>
internal sealed class TestMapperNullTableName : DataModelMapperBase<TestEntityDataModel>
{
    protected override void ConfigureInternal(MapperOptions<TestEntityDataModel> mapperOptions)
    {
        // Intentionally not calling MapTable to leave TableName as null
        mapperOptions.MapColumn(x => x.CustomField);
    }

    public override void MapBinaryImporter(NpgsqlBinaryImporter importer, TestEntityDataModel model)
    {
        // Not implemented for tests
    }
}
