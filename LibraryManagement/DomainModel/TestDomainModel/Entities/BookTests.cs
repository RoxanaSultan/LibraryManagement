using DomainModel.Entities;

namespace TestDomainModel.Entities;

public class BookTests
{
    [Fact]
    public void NewBook_ShouldInitializeCollections()
    {
        // Act
        var book = new Book();

        // Assert
        Assert.NotNull(book.Authors);
        Assert.NotNull(book.ExplicitDomains);
        Assert.NotNull(book.Editions);
    }

    [Fact]
    public void Book_ShouldLinkToAuthorsCorrectly()
    {
        // Arrange
        var book = new Book { Title = "C# Basics" };
        var author = new Author { Name = "John Smith" };

        // Act
        book.Authors.Add(author);

        // Assert
        Assert.Contains(author, book.Authors);
    }

    [Fact]
    public void Book_ShouldHoldMultipleEditions()
    {
        // Arrange
        var book = new Book { Title = "C# Basics" };

        var ed1 = new Edition
        {
            Publisher = "Editura All",
            Year = 2000,
            EditionNumber = "1",
            PageCount = 300,
            BookType = "Paperback",
            InitialStock = 100,
            CurrentStock = 100,
            ReadingRoomOnlyCount = 0
        };

        var ed2 = new Edition
        {
            Publisher = "Editura All",
            Year = 2010,
            EditionNumber = "3",
            PageCount = 320,
            BookType = "Hardcover",
            InitialStock = 80,
            CurrentStock = 80,
            ReadingRoomOnlyCount = 0
        };

        // Act
        book.Editions.Add(ed1);
        book.Editions.Add(ed2);

        // Assert
        Assert.Equal(2, book.Editions.Count);
        Assert.Contains(ed1, book.Editions);
        Assert.Contains(ed2, book.Editions);
    }

    [Fact]
    public void HasOverlappingDomains_WhenNoDomains_ReturnsFalse()
    {
        // Arrange
        var book = new Book { Title = "No Domains" };

        // Act
        var result = book.HasOverlappingDomains();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasOverlappingDomains_WhenSingleDomain_ReturnsFalse()
    {
        // Arrange
        var book = new Book { Title = "One Domain" };
        book.ExplicitDomains.Add(new Domain { Id = 1, Name = "Root" });

        // Act
        var result = book.HasOverlappingDomains();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasOverlappingDomains_WhenTwoUnrelatedDomains_ReturnsFalse()
    {
        // Arrange: doua domenii fara relatie parinte-copil
        var book = new Book { Title = "Unrelated Domains" };

        var d1 = new Domain { Id = 1, Name = "A" };
        var d2 = new Domain { Id = 2, Name = "B" };

        book.ExplicitDomains.Add(d1);
        book.ExplicitDomains.Add(d2);

        // Act
        var result = book.HasOverlappingDomains();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasOverlappingDomains_WhenOneIsAncestorOfAnother_ReturnsTrue()
    {
        // Arrange: A este stramos pentru B (B.ParentDomain = A)
        var book = new Book { Title = "Overlapping Domains" };

        var ancestor = new Domain { Id = 10, Name = "A" };
        var child = new Domain { Id = 11, Name = "B", ParentDomain = ancestor, ParentDomainId = ancestor.Id };

        book.ExplicitDomains.Add(ancestor);
        book.ExplicitDomains.Add(child);

        // Act
        var result = book.HasOverlappingDomains();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasOverlappingDomains_WhenOverlapExistsAmongMany_ReturnsTrue()
    {
        // Arrange: Root <- Mid <- Leaf; Root si Leaf sunt ambele in ExplicitDomains => overlap
        var book = new Book { Title = "Many Domains" };

        var root = new Domain { Id = 1, Name = "Root" };
        var mid = new Domain { Id = 2, Name = "Mid", ParentDomain = root, ParentDomainId = root.Id };
        var leaf = new Domain { Id = 3, Name = "Leaf", ParentDomain = mid, ParentDomainId = mid.Id };

        var unrelated = new Domain { Id = 50, Name = "Unrelated" };

        book.ExplicitDomains.Add(unrelated);
        book.ExplicitDomains.Add(root);
        book.ExplicitDomains.Add(leaf);

        // Act
        var result = book.HasOverlappingDomains();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasOverlappingDomains_WhenDomainsAreInverseOrderStillReturnsTrue()
    {
        // Arrange: lista e [leaf, root], tot trebuie sa dea true din cauza buclelor nested
        var book = new Book { Title = "Inverse Order" };

        var root = new Domain { Id = 100, Name = "Root" };
        var leaf = new Domain { Id = 101, Name = "Leaf", ParentDomain = root, ParentDomainId = root.Id };

        book.ExplicitDomains.Add(leaf);
        book.ExplicitDomains.Add(root);

        // Act
        var result = book.HasOverlappingDomains();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasOverlappingDomains_WhenDuplicateIdsExist_ReturnsFalse()
    {
        // Arrange: d1.Id == d2.Id => continue; fara relatie de stramos => false
        var book = new Book { Title = "Duplicate IDs" };

        var d1 = new Domain { Id = 5, Name = "SameId-1" };
        var d2 = new Domain { Id = 5, Name = "SameId-2" };

        book.ExplicitDomains.Add(d1);
        book.ExplicitDomains.Add(d2);

        // Act
        var result = book.HasOverlappingDomains();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasOverlappingDomains_WhenSameReferenceAddedTwice_ReturnsFalse()
    {
        // Arrange: acelasi obiect de 2 ori => d1.Id == d2.Id => continue
        var book = new Book { Title = "Same Reference Twice" };

        var d = new Domain { Id = 99, Name = "Same" };
        book.ExplicitDomains.Add(d);
        book.ExplicitDomains.Add(d);

        // Act
        var result = book.HasOverlappingDomains();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasOverlappingDomains_WhenOnlyChildAndUnrelated_ReturnsFalse()
    {
        // Arrange: ai un copil care are stramos, dar stramosul NU e in ExplicitDomains => nu e overlap intre elementele listei
        var book = new Book { Title = "Child Without Ancestor Present" };

        var root = new Domain { Id = 1, Name = "Root" };
        var child = new Domain { Id = 2, Name = "Child", ParentDomain = root, ParentDomainId = root.Id };
        var unrelated = new Domain { Id = 99, Name = "Unrelated" };

        book.ExplicitDomains.Add(child);
        book.ExplicitDomains.Add(unrelated);

        // Act
        var result = book.HasOverlappingDomains();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetAllAncestors_WhenNoParent_ReturnsEmpty()
    {
        var d = new Domain { Id = 1, Name = "Root" };

        var ancestors = d.GetAllAncestors().ToList();

        Assert.Empty(ancestors);
    }

    [Fact]
    public void GetAllAncestors_WhenHasChain_ReturnsAllAncestorsInOrder()
    {
        var root = new Domain { Id = 1, Name = "Root" };
        var mid = new Domain { Id = 2, Name = "Mid", ParentDomain = root, ParentDomainId = root.Id };
        var leaf = new Domain { Id = 3, Name = "Leaf", ParentDomain = mid, ParentDomainId = mid.Id };

        var ancestors = leaf.GetAllAncestors().ToList();

        Assert.Equal(2, ancestors.Count);
        Assert.Equal(root.Id, ancestors[1].Id); // leaf -> mid -> root (mid primul, root al doilea)
        Assert.Equal(mid.Id, ancestors[0].Id);
    }
}