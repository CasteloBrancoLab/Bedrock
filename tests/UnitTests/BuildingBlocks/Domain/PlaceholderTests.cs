using Xunit;

namespace Bedrock.UnitTests.BuildingBlocks.Domain;

/// <summary>
/// Placeholder test class for interface-only package.
/// Domain package contains only interfaces (IRepository), no implementation code to test.
/// </summary>
public class PlaceholderTests
{
    [Fact]
    public void DomainPackage_ContainsOnlyInterfaces_NoImplementationToTest()
    {
        // This test exists to satisfy test discovery requirements.
        // The Domain package contains only interfaces, which have no implementation to test.
        Assert.True(true);
    }
}
