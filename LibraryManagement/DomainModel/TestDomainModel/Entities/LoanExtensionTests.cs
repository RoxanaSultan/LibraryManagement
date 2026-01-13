using DomainModel.Entities;
using System;
using Xunit;

namespace TestDomainModel.Entities
{
    /// <summary>
    /// Testeaza datele pentru prelungirile de imprumut (Regula LIM).
    /// </summary>
    public class LoanExtensionTests
    {
        [Fact]
        public void Extension_ShouldLinkToLoanCorrectly()
        {
            // Arrange
            var loan = new Loan { Id = 100 };

            // Act
            var extension = new LoanExtension
            {
                LoanId = loan.Id,
                Loan = loan,
                ExtensionDate = DateTime.Now,
                DaysAdded = 7
            };

            // Assert
            Assert.Equal(100, extension.LoanId);
            Assert.Equal(loan, extension.Loan);
        }

        [Theory]
        [InlineData(7)]
        [InlineData(14)]
        [InlineData(30)]
        public void Extension_ShouldStoreDaysAddedCorrectly(int days)
        {
            // Act
            var extension = new LoanExtension { DaysAdded = days };

            // Assert
            Assert.Equal(days, extension.DaysAdded);
        }

        [Fact]
        public void Extension_Date_ShouldBeStoredCorrectly()
        {
            // Arrange
            var testDate = new DateTime(2025, 12, 1);

            // Act
            var extension = new LoanExtension { ExtensionDate = testDate };

            // Assert
            Assert.Equal(testDate, extension.ExtensionDate);
        }
    }
}