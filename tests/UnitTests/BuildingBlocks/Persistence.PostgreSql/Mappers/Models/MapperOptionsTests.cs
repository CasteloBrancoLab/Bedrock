using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;

public class MapperOptionsTests : TestBase
{
    public MapperOptionsTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void MapTable_ShouldSetSchemaAndTableName()
    {
        // Arrange
        LogArrange("Creating MapperOptions instance");
        MapperOptions<TestDataModel> options = new();

        // Act
        LogAct("Mapping table with schema and name");
        MapperOptions<TestDataModel> result = options.MapTable("public", "test_table");

        // Assert
        LogAssert("Verifying schema and table name are set");
        options.TableSchema.ShouldBe("public");
        options.TableName.ShouldBe("test_table");
        result.ShouldBe(options); // fluent api returns same instance
    }

    [Fact]
    public void MapTable_WithNullSchema_ShouldSetOnlyTableName()
    {
        // Arrange
        LogArrange("Creating MapperOptions instance");
        MapperOptions<TestDataModel> options = new();

        // Act
        LogAct("Mapping table with null schema");
        options.MapTable(null, "test_table");

        // Assert
        LogAssert("Verifying only table name is set");
        options.TableSchema.ShouldBeNull();
        options.TableName.ShouldBe("test_table");
    }

    [Fact]
    public void MapColumn_WithPropertyNameColumnNameAndType_ShouldAddToFieldDictionary()
    {
        // Arrange
        LogArrange("Creating MapperOptions instance");
        MapperOptions<TestDataModel> options = new();

        // Act
        LogAct("Mapping column with explicit names and type");
        options.MapColumn("CustomProperty", "custom_column", typeof(string));

        // Assert
        LogAssert("Verifying column is added to field dictionary");
        options.FieldDictionary.ShouldContainKey("CustomProperty");
        options.FieldDictionary["CustomProperty"].ColumnName.ShouldBe("custom_column");
        options.FieldDictionary["CustomProperty"].Type.ShouldBe(typeof(string));
    }

    [Fact]
    public void MapColumn_WithGenericTypeAndPropertyName_ShouldAddToFieldDictionary()
    {
        // Arrange
        LogArrange("Creating MapperOptions instance");
        MapperOptions<TestDataModel> options = new();

        // Act
        LogAct("Mapping column with generic type");
        options.MapColumn<int>("Count", "count_column");

        // Assert
        LogAssert("Verifying column is added with correct type");
        options.FieldDictionary.ShouldContainKey("Count");
        options.FieldDictionary["Count"].ColumnName.ShouldBe("count_column");
        options.FieldDictionary["Count"].Type.ShouldBe(typeof(int));
    }

    [Fact]
    public void MapColumn_WithGenericTypeAndAutoSnakeCase_ShouldConvertToSnakeCase()
    {
        // Arrange
        LogArrange("Creating MapperOptions instance");
        MapperOptions<TestDataModel> options = new();

        // Act
        LogAct("Mapping column with auto snake case conversion");
        options.MapColumn<string>("CustomProperty");

        // Assert
        LogAssert("Verifying column name is converted to snake_case");
        options.FieldDictionary.ShouldContainKey("CustomProperty");
        options.FieldDictionary["CustomProperty"].ColumnName.ShouldBe("custom_property");
    }

    [Fact]
    public void MapColumn_WithExpression_ShouldExtractPropertyNameAndType()
    {
        // Arrange
        LogArrange("Creating MapperOptions instance");
        MapperOptions<TestDataModel> options = new();

        // Act
        LogAct("Mapping column using expression");
        options.MapColumn(x => x.CustomProperty, "custom_col");

        // Assert
        LogAssert("Verifying property name and type are extracted from expression");
        options.FieldDictionary.ShouldContainKey("CustomProperty");
        options.FieldDictionary["CustomProperty"].ColumnName.ShouldBe("custom_col");
        options.FieldDictionary["CustomProperty"].Type.ShouldBe(typeof(string));
    }

    [Fact]
    public void MapColumn_WithExpressionAndColumnType_ShouldOverrideType()
    {
        // Arrange
        LogArrange("Creating MapperOptions instance");
        MapperOptions<TestDataModel> options = new();

        // Act
        LogAct("Mapping column with expression and explicit type");
        options.MapColumn(x => x.CustomProperty, typeof(int));

        // Assert
        LogAssert("Verifying type is overridden");
        options.FieldDictionary.ShouldContainKey("CustomProperty");
        options.FieldDictionary["CustomProperty"].ColumnName.ShouldBe("custom_property");
        options.FieldDictionary["CustomProperty"].Type.ShouldBe(typeof(int));
    }

    [Fact]
    public void MapColumn_WithExpressionAndGenericType_ShouldUseGenericType()
    {
        // Arrange
        LogArrange("Creating MapperOptions instance");
        MapperOptions<TestDataModel> options = new();

        // Act
        LogAct("Mapping column with expression and generic type");
        options.MapColumn<string, decimal>(x => x.CustomProperty);

        // Assert
        LogAssert("Verifying generic type is used");
        options.FieldDictionary.ShouldContainKey("CustomProperty");
        options.FieldDictionary["CustomProperty"].ColumnName.ShouldBe("custom_property");
        options.FieldDictionary["CustomProperty"].Type.ShouldBe(typeof(decimal));
    }

    [Fact]
    public void MapColumn_WithExpressionOnly_ShouldInferEverything()
    {
        // Arrange
        LogArrange("Creating MapperOptions instance");
        MapperOptions<TestDataModel> options = new();

        // Act
        LogAct("Mapping column with expression only");
        options.MapColumn(x => x.CustomProperty);

        // Assert
        LogAssert("Verifying all properties are inferred");
        options.FieldDictionary.ShouldContainKey("CustomProperty");
        options.FieldDictionary["CustomProperty"].ColumnName.ShouldBe("custom_property");
        options.FieldDictionary["CustomProperty"].Type.ShouldBe(typeof(string));
    }

    [Fact]
    public void AutoMapColumns_ShouldMapAllDeclaredProperties()
    {
        // Arrange
        LogArrange("Creating MapperOptions instance");
        MapperOptions<TestDataModel> options = new();

        // Act
        LogAct("Auto mapping all columns");
        options.AutoMapColumns();

        // Assert
        LogAssert("Verifying declared properties are mapped");
        options.FieldDictionary.ShouldContainKey("CustomProperty");
        options.FieldDictionary["CustomProperty"].ColumnName.ShouldBe("custom_property");
        options.FieldDictionary["CustomProperty"].Type.ShouldBe(typeof(string));

        options.FieldDictionary.ShouldContainKey("IntValue");
        options.FieldDictionary["IntValue"].ColumnName.ShouldBe("int_value");
        options.FieldDictionary["IntValue"].Type.ShouldBe(typeof(int));
    }

    [Fact]
    public void AutoMapColumns_ShouldNotMapBaseClassProperties()
    {
        // Arrange
        LogArrange("Creating MapperOptions instance");
        MapperOptions<TestDataModel> options = new();

        // Act
        LogAct("Auto mapping columns");
        options.AutoMapColumns();

        // Assert
        LogAssert("Verifying base class properties are not mapped");
        options.FieldDictionary.ShouldNotContainKey("Id");
        options.FieldDictionary.ShouldNotContainKey("TenantCode");
        options.FieldDictionary.ShouldNotContainKey("CreatedBy");
    }

    [Fact]
    public void FieldDictionary_ShouldBeReadOnly()
    {
        // Arrange
        LogArrange("Creating MapperOptions instance with mapped column");
        MapperOptions<TestDataModel> options = new();
        options.MapColumn(x => x.CustomProperty);

        // Act
        LogAct("Getting FieldDictionary");
        var fieldDictionary = options.FieldDictionary;

        // Assert
        LogAssert("Verifying dictionary is read-only");
        fieldDictionary.ShouldBeOfType<System.Collections.ObjectModel.ReadOnlyDictionary<string, ColumnMap>>();
    }

    [Fact]
    public void FluentApi_ShouldAllowChaining()
    {
        // Arrange
        LogArrange("Creating MapperOptions instance");
        MapperOptions<TestDataModel> options = new();

        // Act
        LogAct("Chaining multiple fluent calls");
        MapperOptions<TestDataModel> result = options
            .MapTable("schema", "table")
            .MapColumn(x => x.CustomProperty)
            .MapColumn(x => x.IntValue);

        // Assert
        LogAssert("Verifying all operations were applied");
        result.ShouldBe(options);
        options.TableSchema.ShouldBe("schema");
        options.TableName.ShouldBe("table");
        options.FieldDictionary.Count.ShouldBe(2);
    }

    [Fact]
    public void AutoMapColumns_ShouldNotMapReadOnlyProperties()
    {
        // Arrange
        LogArrange("Creating MapperOptions for model with read-only property");
        MapperOptions<TestDataModelWithSpecialProperties> options = new();

        // Act
        LogAct("Auto mapping columns");
        options.AutoMapColumns();

        // Assert
        LogAssert("Verifying read-only property is not mapped");
        options.FieldDictionary.ShouldNotContainKey("ReadOnlyProperty");
        options.FieldDictionary.ShouldContainKey("NormalProperty");
    }

    [Fact]
    public void AutoMapColumns_ShouldNotMapWriteOnlyProperties()
    {
        // Arrange
        LogArrange("Creating MapperOptions for model with write-only property");
        MapperOptions<TestDataModelWithSpecialProperties> options = new();

        // Act
        LogAct("Auto mapping columns");
        options.AutoMapColumns();

        // Assert
        LogAssert("Verifying write-only property is not mapped");
        options.FieldDictionary.ShouldNotContainKey("WriteOnlyProperty");
    }

    [Fact]
    public void AutoMapColumns_ShouldNotMapIndexerProperties()
    {
        // Arrange
        LogArrange("Creating MapperOptions for model with indexer");
        MapperOptions<TestDataModelWithSpecialProperties> options = new();

        // Act
        LogAct("Auto mapping columns");
        options.AutoMapColumns();

        // Assert
        LogAssert("Verifying indexer is not mapped");
        // Indexers are named "Item" in reflection
        options.FieldDictionary.ShouldNotContainKey("Item");
    }

    [Fact]
    public void AutoMapColumns_ShouldReturnSameInstanceForFluentApi()
    {
        // Arrange
        LogArrange("Creating MapperOptions instance");
        MapperOptions<TestDataModel> options = new();

        // Act
        LogAct("Auto mapping columns");
        MapperOptions<TestDataModel> result = options.AutoMapColumns();

        // Assert
        LogAssert("Verifying same instance is returned");
        result.ShouldBeSameAs(options);
    }
}

// Test data model for MapperOptions tests
public class TestDataModel : DataModelBase
{
    public string CustomProperty { get; set; } = null!;
    public int IntValue { get; set; }
}

/// <summary>
/// Test data model with properties that should be excluded from AutoMapColumns
/// </summary>
public class TestDataModelWithSpecialProperties : DataModelBase
{
    // This should be mapped - normal read/write property
    public string NormalProperty { get; set; } = null!;

    // This should NOT be mapped - read-only property (no setter)
    public string ReadOnlyProperty => "readonly";

    // This should NOT be mapped - write-only property (no getter)
    public string WriteOnlyProperty { set { _ = value; } }

    // This is an indexer - should NOT be mapped (has index parameters)
    public string this[int index]
    {
        get => index.ToString();
        set { _ = value; }
    }
}
