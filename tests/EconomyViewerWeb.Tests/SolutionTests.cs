namespace EconomyViewerWeb.Tests;

public class SolutionTests
{
    [Fact]
    public void DomainAssemblyReferenceExists()
    {
        Assert.NotNull(typeof(Domain.AssemblyReference));
    }
}
