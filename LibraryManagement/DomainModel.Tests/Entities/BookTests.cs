using DomainModel.Entities;
using Xunit;
using System.Collections.Generic;

public class BookTests
{
    [Fact]
    public void GetAllDomains_IncludesAncestors()
    {
        var stiinta = new Domain { Id = 1, Name = "Stiinta" };
        var informatica = new Domain { Id = 2, Name = "Informatica", ParentDomain = stiinta };
        var bazeDeDate = new Domain { Id = 3, Name = "Baze de date", ParentDomain = informatica };

        var book = new Book
        {
            ExplicitDomains = new List<Domain> { bazeDeDate }
        };

        var allDomains = book.GetAllDomains();

        Assert.Contains(bazeDeDate, allDomains);
        Assert.Contains(informatica, allDomains);
        Assert.Contains(stiinta, allDomains);
        Assert.Equal(3, new HashSet<Domain>(allDomains).Count);
    }
}