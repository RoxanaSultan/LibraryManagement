using DomainModel.Entities;

namespace TestDomainModel.Entities;

public class LoanTests
{
    [Fact]
    public void Loan_ShouldBeOverdue_WhenDueDateIsPast()
    {
        // Arrange
        var loan = new Loan
        {
            DueDate = DateTime.Now.AddDays(-1),
            ReturnDate = null
        };

        // Act
        bool isOverdue = DateTime.Now > loan.DueDate && loan.ReturnDate == null;

        // Assert
        Assert.True(isOverdue);
    }

    [Fact]
    public void Loan_ShouldNotBeOverdue_WhenReturned()
    {
        // Arrange
        var loan = new Loan
        {
            DueDate = DateTime.Now.AddDays(-1),
            ReturnDate = DateTime.Now.AddDays(-2)
        };

        // Act
        bool isOverdue = DateTime.Now > loan.DueDate && loan.ReturnDate == null;

        // Assert
        Assert.False(isOverdue);
    }
}