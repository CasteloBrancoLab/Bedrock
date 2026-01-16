using Bedrock.BuildingBlocks.Core.Filterings;
using Bedrock.BuildingBlocks.Core.Filterings.Enums;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Core.Sortings;
using Bedrock.BuildingBlocks.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using SortDirection = Bedrock.BuildingBlocks.Core.Sortings.Enums.SortDirection;

namespace Bedrock.UnitTests.BuildingBlocks.Core.Paginations;

public class PaginationInfoTests : TestBase
{
    public PaginationInfoTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    #region Create Tests

    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        // Arrange
        LogArrange("Setting up pagination");
        var page = 3;
        var pageSize = 10;

        // Act
        LogAct("Creating PaginationInfo");
        var pagination = PaginationInfo.Create(page, pageSize);

        // Assert
        LogAssert("Verifying all properties");
        pagination.Page.ShouldBe(page);
        pagination.PageSize.ShouldBe(pageSize);
        pagination.Index.ShouldBe(2); // Page - 1
        pagination.Offset.ShouldBe(20); // Index * PageSize
        pagination.SortCollection.ShouldBeNull();
        pagination.FilterCollection.ShouldBeNull();
        pagination.HasSort.ShouldBeFalse();
        pagination.HasFilter.ShouldBeFalse();
        pagination.IsUnbounded.ShouldBeFalse();
        LogInfo("Pagination: {0}", pagination);
    }

    [Theory]
    [InlineData(1, 10, 0, 0)]
    [InlineData(2, 10, 1, 10)]
    [InlineData(3, 10, 2, 20)]
    [InlineData(1, 20, 0, 0)]
    [InlineData(5, 20, 4, 80)]
    public void Create_ShouldCalculateIndexAndOffsetCorrectly(int page, int pageSize, int expectedIndex, int expectedOffset)
    {
        // Arrange
        LogArrange("Creating pagination");
        LogInfo("page={0}, pageSize={1}", page, pageSize);

        // Act
        LogAct("Creating PaginationInfo");
        var pagination = PaginationInfo.Create(page, pageSize);

        // Assert
        LogAssert("Verifying Index and Offset");
        pagination.Index.ShouldBe(expectedIndex);
        pagination.Offset.ShouldBe(expectedOffset);
        LogInfo("Index={0}, Offset={1}", pagination.Index, pagination.Offset);
    }

    [Fact]
    public void Create_WithZeroPage_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        LogArrange("Preparing zero page");

        // Act & Assert
        LogAct("Creating PaginationInfo with zero page");
        var exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            PaginationInfo.Create(0, 10));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("page");
        LogInfo("Exception thrown: {0}", exception.Message);
    }

    [Fact]
    public void Create_WithNegativePage_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        LogArrange("Preparing negative page");

        // Act & Assert
        LogAct("Creating PaginationInfo with negative page");
        var exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            PaginationInfo.Create(-1, 10));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("page");
    }

    [Fact]
    public void Create_WithZeroPageSize_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        LogArrange("Preparing zero pageSize");

        // Act & Assert
        LogAct("Creating PaginationInfo with zero pageSize");
        var exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            PaginationInfo.Create(1, 0));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("pageSize");
    }

    [Fact]
    public void Create_WithNegativePageSize_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        LogArrange("Preparing negative pageSize");

        // Act & Assert
        LogAct("Creating PaginationInfo with negative pageSize");
        var exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            PaginationInfo.Create(1, -10));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("pageSize");
    }

    #endregion

    #region Create With Collections Tests

    [Fact]
    public void Create_WithCollections_ZeroPage_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        LogArrange("Preparing zero page with collections");
        var sorts = new[] { SortInfo.Create("Name", SortDirection.Ascending) };

        // Act & Assert
        LogAct("Creating PaginationInfo with zero page");
        var exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            PaginationInfo.Create(0, 10, sorts, null));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("page");
    }

    [Fact]
    public void Create_WithCollections_ZeroPageSize_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        LogArrange("Preparing zero pageSize with collections");
        var filters = new[] { FilterInfo.Create("Status", FilterOperator.Equals, "Active") };

        // Act & Assert
        LogAct("Creating PaginationInfo with zero pageSize");
        var exception = Should.Throw<ArgumentOutOfRangeException>(() =>
            PaginationInfo.Create(1, 0, null, filters));

        LogAssert("Verifying exception");
        exception.ParamName.ShouldBe("pageSize");
    }

    [Fact]
    public void Create_WithSortAndFilter_ShouldSetAllProperties()
    {
        // Arrange
        LogArrange("Setting up pagination with sort and filter");
        var sorts = new[] { SortInfo.Create("Name", SortDirection.Ascending) };
        var filters = new[] { FilterInfo.Create("Status", FilterOperator.Equals, "Active") };

        // Act
        LogAct("Creating PaginationInfo with collections");
        var pagination = PaginationInfo.Create(1, 20, sorts, filters);

        // Assert
        LogAssert("Verifying collections");
        pagination.SortCollection.ShouldBe(sorts);
        pagination.FilterCollection.ShouldBe(filters);
        pagination.HasSort.ShouldBeTrue();
        pagination.HasFilter.ShouldBeTrue();
        LogInfo("Pagination with collections: {0}", pagination);
    }

    [Fact]
    public void Create_WithNullCollections_ShouldWork()
    {
        // Arrange
        LogArrange("Setting up pagination with null collections");

        // Act
        LogAct("Creating PaginationInfo with null collections");
        var pagination = PaginationInfo.Create(1, 20, null, null);

        // Assert
        LogAssert("Verifying null collections");
        pagination.SortCollection.ShouldBeNull();
        pagination.FilterCollection.ShouldBeNull();
        pagination.HasSort.ShouldBeFalse();
        pagination.HasFilter.ShouldBeFalse();
    }

    [Fact]
    public void Create_WithEmptyCollections_ShouldWork()
    {
        // Arrange
        LogArrange("Setting up pagination with empty collections");
        var sorts = Array.Empty<SortInfo>();
        var filters = Array.Empty<FilterInfo>();

        // Act
        LogAct("Creating PaginationInfo with empty collections");
        var pagination = PaginationInfo.Create(1, 20, sorts, filters);

        // Assert
        LogAssert("Verifying empty collections");
        pagination.SortCollection.ShouldBe(sorts);
        pagination.FilterCollection.ShouldBe(filters);
        pagination.HasSort.ShouldBeFalse();
        pagination.HasFilter.ShouldBeFalse();
    }

    #endregion

    #region All and CreateAll Tests

    [Fact]
    public void All_ShouldReturnUnboundedPagination()
    {
        // Arrange
        LogArrange("Getting All pagination");

        // Act
        LogAct("Accessing PaginationInfo.All");
        var pagination = PaginationInfo.All;

        // Assert
        LogAssert("Verifying unbounded pagination");
        pagination.Page.ShouldBe(1);
        pagination.PageSize.ShouldBe(int.MaxValue);
        pagination.IsUnbounded.ShouldBeTrue();
        pagination.SortCollection.ShouldBeNull();
        pagination.FilterCollection.ShouldBeNull();
        LogInfo("All pagination: {0}", pagination);
    }

    [Fact]
    public void CreateAll_ShouldReturnUnboundedPagination()
    {
        // Arrange
        LogArrange("Creating All pagination");

        // Act
        LogAct("Calling CreateAll");
        var pagination = PaginationInfo.CreateAll();

        // Assert
        LogAssert("Verifying unbounded pagination");
        pagination.Page.ShouldBe(1);
        pagination.PageSize.ShouldBe(int.MaxValue);
        pagination.IsUnbounded.ShouldBeTrue();
    }

    [Fact]
    public void CreateAll_WithCollections_ShouldSetCollections()
    {
        // Arrange
        LogArrange("Creating All with collections");
        var sorts = new[] { SortInfo.Create("Name", SortDirection.Ascending) };
        var filters = new[] { FilterInfo.Create("Active", FilterOperator.Equals, "true") };

        // Act
        LogAct("Calling CreateAll with collections");
        var pagination = PaginationInfo.CreateAll(sorts, filters);

        // Assert
        LogAssert("Verifying collections on unbounded");
        pagination.IsUnbounded.ShouldBeTrue();
        pagination.SortCollection.ShouldBe(sorts);
        pagination.FilterCollection.ShouldBe(filters);
        pagination.HasSort.ShouldBeTrue();
        pagination.HasFilter.ShouldBeTrue();
    }

    #endregion

    #region CreateFromExistingInfo Tests

    [Fact]
    public void CreateFromExistingInfo_ShouldSetAllProperties()
    {
        // Arrange
        LogArrange("Setting up existing info");
        var sorts = new[] { SortInfo.Create("Name", SortDirection.Ascending) };
        var filters = new[] { FilterInfo.Create("Status", FilterOperator.Equals, "Active") };

        // Act
        LogAct("Creating from existing info");
        var pagination = PaginationInfo.CreateFromExistingInfo(2, 15, sorts, filters);

        // Assert
        LogAssert("Verifying all properties");
        pagination.Page.ShouldBe(2);
        pagination.PageSize.ShouldBe(15);
        pagination.SortCollection.ShouldBe(sorts);
        pagination.FilterCollection.ShouldBe(filters);
    }

    [Fact]
    public void CreateFromExistingInfo_WithInvalidValues_ShouldNotThrow()
    {
        // Arrange
        LogArrange("Preparing invalid values for existing info");

        // Act
        LogAct("Creating from existing with zero page");
        var pagination = PaginationInfo.CreateFromExistingInfo(0, 0, null, null);

        // Assert
        LogAssert("Verifying no exception");
        pagination.Page.ShouldBe(0);
        pagination.PageSize.ShouldBe(0);
        LogInfo("Invalid values accepted in CreateFromExistingInfo");
    }

    #endregion

    #region With Methods Tests

    [Fact]
    public void WithSortCollection_ShouldReturnNewInstanceWithSorts()
    {
        // Arrange
        LogArrange("Creating pagination and sorts");
        var pagination = PaginationInfo.Create(1, 10);
        var sorts = new[] { SortInfo.Create("Name", SortDirection.Ascending) };

        // Act
        LogAct("Adding sort collection");
        var newPagination = pagination.WithSortCollection(sorts);

        // Assert
        LogAssert("Verifying new instance");
        newPagination.SortCollection.ShouldBe(sorts);
        newPagination.HasSort.ShouldBeTrue();
        newPagination.Page.ShouldBe(pagination.Page);
        newPagination.PageSize.ShouldBe(pagination.PageSize);
        pagination.SortCollection.ShouldBeNull(); // Original unchanged
    }

    [Fact]
    public void WithFilterCollection_ShouldReturnNewInstanceWithFilters()
    {
        // Arrange
        LogArrange("Creating pagination and filters");
        var pagination = PaginationInfo.Create(1, 10);
        var filters = new[] { FilterInfo.Create("Status", FilterOperator.Equals, "Active") };

        // Act
        LogAct("Adding filter collection");
        var newPagination = pagination.WithFilterCollection(filters);

        // Assert
        LogAssert("Verifying new instance");
        newPagination.FilterCollection.ShouldBe(filters);
        newPagination.HasFilter.ShouldBeTrue();
        newPagination.Page.ShouldBe(pagination.Page);
        newPagination.PageSize.ShouldBe(pagination.PageSize);
        pagination.FilterCollection.ShouldBeNull(); // Original unchanged
    }

    [Fact]
    public void WithSortCollection_OnAll_ShouldReturnUnboundedWithSorts()
    {
        // Arrange
        LogArrange("Creating All pagination");
        var sorts = new[] { SortInfo.Create("Name", SortDirection.Ascending) };

        // Act
        LogAct("Adding sorts to All");
        var pagination = PaginationInfo.All.WithSortCollection(sorts);

        // Assert
        LogAssert("Verifying unbounded with sorts");
        pagination.IsUnbounded.ShouldBeTrue();
        pagination.HasSort.ShouldBeTrue();
    }

    [Fact]
    public void WithFilterCollection_PreservesSortCollection()
    {
        // Arrange
        LogArrange("Creating pagination with sorts");
        var sorts = new[] { SortInfo.Create("Name", SortDirection.Ascending) };
        var filters = new[] { FilterInfo.Create("Status", FilterOperator.Equals, "Active") };
        var pagination = PaginationInfo.Create(1, 10, sorts, null);

        // Act
        LogAct("Adding filters");
        var newPagination = pagination.WithFilterCollection(filters);

        // Assert
        LogAssert("Verifying sorts preserved");
        newPagination.SortCollection.ShouldBe(sorts);
        newPagination.FilterCollection.ShouldBe(filters);
    }

    [Fact]
    public void WithSortCollection_PreservesFilterCollection()
    {
        // Arrange
        LogArrange("Creating pagination with filters");
        var sorts = new[] { SortInfo.Create("Name", SortDirection.Ascending) };
        var filters = new[] { FilterInfo.Create("Status", FilterOperator.Equals, "Active") };
        var pagination = PaginationInfo.Create(1, 10, null, filters);

        // Act
        LogAct("Adding sorts");
        var newPagination = pagination.WithSortCollection(sorts);

        // Assert
        LogAssert("Verifying filters preserved");
        newPagination.SortCollection.ShouldBe(sorts);
        newPagination.FilterCollection.ShouldBe(filters);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSamePageAndPageSize_ShouldReturnTrue()
    {
        // Arrange
        LogArrange("Creating two PaginationInfos with same values");
        var pagination1 = PaginationInfo.Create(2, 20);
        var pagination2 = PaginationInfo.Create(2, 20);

        // Act
        LogAct("Comparing for equality");
        var areEqual = pagination1.Equals(pagination2);

        // Assert
        LogAssert("Verifying equality");
        areEqual.ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentPage_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating PaginationInfos with different pages");
        var pagination1 = PaginationInfo.Create(1, 20);
        var pagination2 = PaginationInfo.Create(2, 20);

        // Act
        LogAct("Comparing for equality");
        var areEqual = pagination1.Equals(pagination2);

        // Assert
        LogAssert("Verifying inequality");
        areEqual.ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithDifferentPageSize_ShouldReturnFalse()
    {
        // Arrange
        LogArrange("Creating PaginationInfos with different pageSizes");
        var pagination1 = PaginationInfo.Create(1, 10);
        var pagination2 = PaginationInfo.Create(1, 20);

        // Act
        LogAct("Comparing for equality");
        var areEqual = pagination1.Equals(pagination2);

        // Assert
        LogAssert("Verifying inequality");
        areEqual.ShouldBeFalse();
    }

    [Fact]
    public void Equals_IgnoresSortAndFilterCollections()
    {
        // Arrange - equality is based only on Page and PageSize
        LogArrange("Creating PaginationInfos with same page/size but different collections");
        var sorts = new[] { SortInfo.Create("Name", SortDirection.Ascending) };
        var pagination1 = PaginationInfo.Create(1, 10);
        var pagination2 = PaginationInfo.Create(1, 10, sorts, null);

        // Act
        LogAct("Comparing for equality");
        var areEqual = pagination1.Equals(pagination2);

        // Assert
        LogAssert("Verifying equality ignores collections");
        areEqual.ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithObjectParameter_ShouldWork()
    {
        // Arrange
        LogArrange("Creating PaginationInfo and objects");
        var pagination = PaginationInfo.Create(1, 10);
        object objSame = PaginationInfo.Create(1, 10);
        object objDifferent = PaginationInfo.Create(2, 20);
        object? objNull = null;
        object objWrongType = "not a pagination";

        // Act & Assert
        LogAct("Testing Equals with various object types");
        pagination.Equals(objSame).ShouldBeTrue();
        pagination.Equals(objDifferent).ShouldBeFalse();
        pagination.Equals(objNull).ShouldBeFalse();
        pagination.Equals(objWrongType).ShouldBeFalse();
        LogAssert("Object equality tests passed");
    }

    [Fact]
    public void EqualityOperator_ShouldWork()
    {
        // Arrange
        LogArrange("Creating PaginationInfos");
        var pagination1 = PaginationInfo.Create(1, 10);
        var pagination2 = PaginationInfo.Create(1, 10);
        var pagination3 = PaginationInfo.Create(2, 20);

        // Act & Assert
        LogAct("Testing operators");
        (pagination1 == pagination2).ShouldBeTrue();
        (pagination1 != pagination3).ShouldBeTrue();
        LogAssert("Operators work correctly");
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_SameValues_ShouldReturnSameHash()
    {
        // Arrange
        LogArrange("Creating two PaginationInfos with same values");
        var pagination1 = PaginationInfo.Create(2, 20);
        var pagination2 = PaginationInfo.Create(2, 20);

        // Act
        LogAct("Getting hash codes");
        var hash1 = pagination1.GetHashCode();
        var hash2 = pagination2.GetHashCode();

        // Assert
        LogAssert("Verifying hash codes are equal");
        hash1.ShouldBe(hash2);
    }

    [Fact]
    public void GetHashCode_DifferentValues_ShouldReturnDifferentHash()
    {
        // Arrange
        LogArrange("Creating two different PaginationInfos");
        var pagination1 = PaginationInfo.Create(1, 10);
        var pagination2 = PaginationInfo.Create(2, 20);

        // Act
        LogAct("Getting hash codes");
        var hash1 = pagination1.GetHashCode();
        var hash2 = pagination2.GetHashCode();

        // Assert
        LogAssert("Verifying hash codes are different");
        hash1.ShouldNotBe(hash2);
    }

    [Fact]
    public void GetHashCode_ShouldCombinePageAndPageSize()
    {
        // Arrange
        LogArrange("Creating PaginationInfos differing in single fields");
        var basePagination = PaginationInfo.Create(1, 10);
        var diffPage = PaginationInfo.Create(2, 10);
        var diffPageSize = PaginationInfo.Create(1, 20);

        // Act
        LogAct("Getting hash codes");
        var baseHash = basePagination.GetHashCode();
        var pageHash = diffPage.GetHashCode();
        var pageSizeHash = diffPageSize.GetHashCode();

        // Assert
        LogAssert("Verifying both fields contribute to hash");
        baseHash.ShouldNotBe(pageHash, "Page should affect hash");
        baseHash.ShouldNotBe(pageSizeHash, "PageSize should affect hash");
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_BasicPagination_ShouldReturnCorrectFormat()
    {
        // Arrange
        LogArrange("Creating basic pagination");
        var pagination = PaginationInfo.Create(3, 10);

        // Act
        LogAct("Calling ToString");
        var result = pagination.ToString();

        // Assert
        LogAssert("Verifying format");
        result.ShouldContain("Page: 3");
        result.ShouldContain("PageSize: 10");
        result.ShouldContain("Index: 2");
        result.ShouldContain("Offset: 20");
        result.ShouldNotContain("SortCollection");
        result.ShouldNotContain("FilterCollection");
        LogInfo("ToString: {0}", result);
    }

    [Fact]
    public void ToString_WithSort_ShouldIncludeSortCollection()
    {
        // Arrange
        LogArrange("Creating pagination with sort");
        var sorts = new[] { SortInfo.Create("Name", SortDirection.Ascending) };
        var pagination = PaginationInfo.Create(1, 10, sorts, null);

        // Act
        LogAct("Calling ToString");
        var result = pagination.ToString();

        // Assert
        LogAssert("Verifying sort in output");
        result.ShouldContain("SortCollection: [");
        result.ShouldContain("Name Ascending");
        result.ShouldContain("]");
    }

    [Fact]
    public void ToString_WithFilter_ShouldIncludeFilterCollection()
    {
        // Arrange
        LogArrange("Creating pagination with filter");
        var filters = new[] { FilterInfo.Create("Status", FilterOperator.Equals, "Active") };
        var pagination = PaginationInfo.Create(1, 10, null, filters);

        // Act
        LogAct("Calling ToString");
        var result = pagination.ToString();

        // Assert
        LogAssert("Verifying filter in output");
        result.ShouldContain("FilterCollection: [");
        result.ShouldContain("Status Equals Active");
        result.ShouldContain("]");
    }

    [Fact]
    public void ToString_WithBothCollections_ShouldIncludeBoth()
    {
        // Arrange
        LogArrange("Creating pagination with both collections");
        var sorts = new[] { SortInfo.Create("Name", SortDirection.Ascending) };
        var filters = new[] { FilterInfo.Create("Status", FilterOperator.Equals, "Active") };
        var pagination = PaginationInfo.Create(1, 10, sorts, filters);

        // Act
        LogAct("Calling ToString");
        var result = pagination.ToString();

        // Assert
        LogAssert("Verifying both collections in output");
        result.ShouldContain("SortCollection: [Name Ascending]");
        result.ShouldContain("FilterCollection: [Status Equals Active]");
        // Verify comma separator
        result.ShouldContain(", ");
    }

    [Fact]
    public void ToString_PartsJoinedWithComma()
    {
        // Arrange - mata mutante de string.Join separador
        LogArrange("Creating pagination for join verification");
        var pagination = PaginationInfo.Create(2, 15);

        // Act
        LogAct("Calling ToString");
        var result = pagination.ToString();

        // Assert
        LogAssert("Verifying comma separator");
        result.ShouldBe("Page: 2, PageSize: 15, Index: 1, Offset: 15");
    }

    [Fact]
    public void ToString_SortCollectionFormat_MustHaveCorrectSeparator()
    {
        // Arrange - mata mutante de string na linha 290 (string.Join ", ")
        LogArrange("Creating pagination with multiple sorts for separator check");
        var sorts = new[]
        {
            SortInfo.Create("Name", SortDirection.Ascending),
            SortInfo.Create("Age", SortDirection.Descending)
        };
        var pagination = PaginationInfo.Create(1, 10, sorts, null);

        // Act
        LogAct("Calling ToString");
        var result = pagination.ToString();

        // Assert - verifica que os sorts estao separados por ", "
        LogAssert("Verifying SortCollection format with separator");
        result.ShouldContain("SortCollection: [Name Ascending, Age Descending]");
    }

    [Fact]
    public void ToString_FilterCollectionFormat_MustHaveCorrectSeparator()
    {
        // Arrange - mata mutante de string na linha 295 (string.Join ", ")
        LogArrange("Creating pagination with multiple filters for separator check");
        var filters = new[]
        {
            FilterInfo.Create("Status", FilterOperator.Equals, "Active"),
            FilterInfo.Create("Name", FilterOperator.Contains, "John")
        };
        var pagination = PaginationInfo.Create(1, 10, null, filters);

        // Act
        LogAct("Calling ToString");
        var result = pagination.ToString();

        // Assert - verifica que os filters estao separados por ", "
        LogAssert("Verifying FilterCollection format with separator");
        result.ShouldContain("FilterCollection: [Status Equals Active, Name Contains John]");
    }

    #endregion

    #region Mutation Killing Tests

    [Fact]
    public void Index_ShouldBePageMinusOne()
    {
        // Arrange
        LogArrange("Creating pagination");
        var pagination = PaginationInfo.Create(5, 10);

        // Act & Assert
        LogAct("Verifying Index calculation");
        pagination.Index.ShouldBe(4); // 5 - 1
        pagination.Index.ShouldBe(pagination.Page - 1);
        LogAssert("Index = Page - 1 verified");
    }

    [Fact]
    public void Offset_ShouldBeIndexTimesPageSize()
    {
        // Arrange
        LogArrange("Creating pagination");
        var pagination = PaginationInfo.Create(3, 15);

        // Act & Assert
        LogAct("Verifying Offset calculation");
        pagination.Offset.ShouldBe(30); // (3-1) * 15
        pagination.Offset.ShouldBe(pagination.Index * pagination.PageSize);
        LogAssert("Offset = Index * PageSize verified");
    }

    [Fact]
    public void IsUnbounded_ShouldBeTrueOnlyForMaxValue()
    {
        // Arrange
        LogArrange("Creating bounded and unbounded paginations");
        var bounded = PaginationInfo.Create(1, 100);
        var almostUnbounded = PaginationInfo.CreateFromExistingInfo(1, int.MaxValue - 1, null, null);
        var unbounded = PaginationInfo.All;

        // Act & Assert
        LogAct("Verifying IsUnbounded");
        bounded.IsUnbounded.ShouldBeFalse();
        almostUnbounded.IsUnbounded.ShouldBeFalse();
        unbounded.IsUnbounded.ShouldBeTrue();
        LogAssert("IsUnbounded correctly checks int.MaxValue");
    }

    [Fact]
    public void HasSort_ShouldCheckNullAndCount()
    {
        // Arrange
        LogArrange("Creating paginations with various sort states");
        var noSort = PaginationInfo.Create(1, 10);
        var emptySort = PaginationInfo.Create(1, 10, Array.Empty<SortInfo>(), null);
        var withSort = PaginationInfo.Create(1, 10, new[] { SortInfo.Create("Name", SortDirection.Ascending) }, null);

        // Act & Assert
        LogAct("Verifying HasSort");
        noSort.HasSort.ShouldBeFalse("Null collection should be false");
        emptySort.HasSort.ShouldBeFalse("Empty collection should be false");
        withSort.HasSort.ShouldBeTrue("Non-empty collection should be true");
        LogAssert("HasSort correctly checks null and count");
    }

    [Fact]
    public void HasFilter_ShouldCheckNullAndCount()
    {
        // Arrange
        LogArrange("Creating paginations with various filter states");
        var noFilter = PaginationInfo.Create(1, 10);
        var emptyFilter = PaginationInfo.Create(1, 10, null, Array.Empty<FilterInfo>());
        var withFilter = PaginationInfo.Create(1, 10, null, new[] { FilterInfo.Create("Status", FilterOperator.Equals, "Active") });

        // Act & Assert
        LogAct("Verifying HasFilter");
        noFilter.HasFilter.ShouldBeFalse("Null collection should be false");
        emptyFilter.HasFilter.ShouldBeFalse("Empty collection should be false");
        withFilter.HasFilter.ShouldBeTrue("Non-empty collection should be true");
        LogAssert("HasFilter correctly checks null and count");
    }

    [Fact]
    public void Equals_AllFieldsMustMatch()
    {
        // Arrange
        LogArrange("Creating PaginationInfos for comprehensive equality check");
        var reference = PaginationInfo.Create(2, 20);
        var diffPageOnly = PaginationInfo.Create(3, 20);
        var diffPageSizeOnly = PaginationInfo.Create(2, 30);

        // Act & Assert
        LogAct("Verifying each field affects equality");
        reference.Equals(diffPageOnly).ShouldBeFalse("Page must affect equality");
        reference.Equals(diffPageSizeOnly).ShouldBeFalse("PageSize must affect equality");
        LogAssert("All equality conditions verified");
    }

    [Fact]
    public void InequalityOperator_ShouldNegateEquals()
    {
        // Arrange
        LogArrange("Creating PaginationInfos for inequality check");
        var pagination1 = PaginationInfo.Create(1, 10);
        var pagination2Same = PaginationInfo.Create(1, 10);
        var pagination3Diff = PaginationInfo.Create(2, 20);

        // Act & Assert
        LogAct("Verifying != negates ==");
        (pagination1 == pagination2Same).ShouldBeTrue();
        (pagination1 != pagination2Same).ShouldBeFalse();
        (pagination1 == pagination3Diff).ShouldBeFalse();
        (pagination1 != pagination3Diff).ShouldBeTrue();
        LogAssert("Inequality operator correctly negates equality");
    }

    #endregion
}
