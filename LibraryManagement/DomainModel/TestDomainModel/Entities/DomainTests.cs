using DomainModel.Entities;

namespace TestDomainModel.Entities;

/// <summary>
/// Testeaza logica ierarhica a entitatii Domain.
/// </summary>
public class DomainTests
{
    [Fact]
    public void Domain_WithoutParent_ShouldHaveNoAncestors()
    {
        // Arrange
        var domain = new Domain { Id = 1, Name = "Science" };

        // Act
        var ancestors = domain.GetAllAncestors();

        // Assert
        Assert.Empty(ancestors);
    }

    [Fact]
    public void SubDomain_ShouldIdentifyParentAsAncestor()
    {
        // Arrange
        var parent = new Domain { Id = 1, Name = "Science" };
        var child = new Domain { Id = 2, Name = "Physics", ParentDomain = parent };

        // Act
        var ancestors = child.GetAllAncestors().ToList();

        // Assert
        Assert.Single(ancestors);
        Assert.Equal("Science", ancestors[0].Name);
    }

    [Fact]
    public void DeepHierarchy_ShouldIdentifyAllAncestors()
    {
        // Arrange
        var root = new Domain { Id = 1, Name = "Science" };
        var mid = new Domain { Id = 2, Name = "Computer Science", ParentDomain = root };
        var leaf = new Domain { Id = 3, Name = "Databases", ParentDomain = mid };

        // Act
        var ancestors = leaf.GetAllAncestors().ToList();

        // Assert
        Assert.Equal(2, ancestors.Count);
        Assert.Contains(ancestors, a => a.Name == "Science");
        Assert.Contains(ancestors, a => a.Name == "Computer Science");
    }
}