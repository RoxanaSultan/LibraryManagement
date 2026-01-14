using DomainModel.Entities;
using DomainModel.Validators;
using FluentValidation.TestHelper;
using System.Collections.Generic;
using Xunit;

namespace TestDomainModel.Validators
{
    /// <summary>
    /// Testeaza regulile de validare pentru entitatea Book.
    /// </summary>
    public class BookValidatorTests
    {
        private const int TestMaxDomains = 3; // Pragul DOMENII pentru teste
        private readonly BookValidator _validator = new BookValidator(TestMaxDomains);

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Should_Have_Error_When_Title_Is_Invalid(string title)
        {
            // Arrange
            var book = new Book { Title = title };

            // Act
            var result = _validator.TestValidate(book);

            // Assert
            result.ShouldHaveValidationErrorFor(b => b.Title);
        }

        [Fact]
        public void Should_Have_Error_When_ExplicitDomains_Is_Empty()
        {
            // Arrange
            var book = new Book { Title = "Valid Title", ExplicitDomains = new List<Domain>() };

            // Act
            var result = _validator.TestValidate(book);

            // Assert
            result.ShouldHaveValidationErrorFor(b => b.ExplicitDomains)
                  .WithErrorMessage("Cartea trebuie sa apartina de cel putin un domeniu.");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)] // Exact limita maxima
        public void Should_Not_Have_Error_When_Domain_Count_Is_Within_Limit(int domainCount)
        {
            // Arrange
            var domains = new List<Domain>();
            for (int i = 0; i < domainCount; i++)
            {
                domains.Add(new Domain { Id = i, Name = "Domain " + i });
            }

            var book = new Book { Title = "Valid Title", ExplicitDomains = domains };

            // Act
            var result = _validator.TestValidate(book);

            // Assert
            result.ShouldNotHaveValidationErrorFor(b => b.ExplicitDomains);
        }

        [Fact]
        public void Should_Have_Error_When_Domain_Count_Exceeds_Limit()
        {
            // Arrange
            var domains = new List<Domain>();
            for (int i = 0; i < 4; i++)
            {
                domains.Add(new Domain { Id = i, Name = "Domain " + i });
            }

            var book = new Book { Title = "Valid Title", ExplicitDomains = domains };

            // Act
            var result = _validator.TestValidate(book);

            // Assert
            result.ShouldHaveValidationErrorFor(b => b.ExplicitDomains)
                  .WithErrorMessage($"O carte nu poate face parte din mai mult de {TestMaxDomains} domenii.");
        }

        [Fact]
        public void Should_Be_Valid_When_All_Conditions_Are_Met()
        {
            // Arrange
            var book = new Book
            {
                Title = "Clean Code",
                ExplicitDomains = new List<Domain> { new Domain { Name = "IT" } }
            };

            // Act
            var result = _validator.TestValidate(book);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}