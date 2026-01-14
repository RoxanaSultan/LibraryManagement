using System;
using System.Collections.Generic;
using DomainModel.Exceptions;
using Xunit;

namespace TestDomainModel.Exceptions;

public class DomainExceptionsTests
{
    // ---------- LibraryException ----------

    [Fact]
    public void LibraryException_InheritsFromException()
    {
        var ex = new LibraryException("msg");
        Assert.IsAssignableFrom<Exception>(ex);
    }

    [Fact]
    public void LibraryException_SetsMessage()
    {
        var ex = new LibraryException("Mesaj");
        Assert.Equal("Mesaj", ex.Message);
    }

    [Fact]
    public void LibraryException_SetsInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new LibraryException("outer", inner);

        Assert.Equal("outer", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    // ---------- DomainConstraintException ----------

    [Fact]
    public void DomainConstraintException_InheritsFromLibraryException()
    {
        var ex = new DomainConstraintException("x");
        Assert.IsAssignableFrom<LibraryException>(ex);
    }

    [Fact]
    public void DomainConstraintException_PreservesMessage()
    {
        var ex = new DomainConstraintException("Dom limit");
        Assert.Equal("Dom limit", ex.Message);
    }

    // ---------- DomainHierarchyException ----------

    [Fact]
    public void DomainHierarchyException_InheritsFromLibraryException()
    {
        var ex = new DomainHierarchyException("x");
        Assert.IsAssignableFrom<LibraryException>(ex);
    }

    [Fact]
    public void DomainHierarchyException_PreservesMessage()
    {
        var ex = new DomainHierarchyException("Hierarchy broken");
        Assert.Equal("Hierarchy broken", ex.Message);
    }

    // ---------- InsufficientStockException ----------

    [Fact]
    public void InsufficientStockException_InheritsFromLibraryException()
    {
        var ex = new InsufficientStockException("x");
        Assert.IsAssignableFrom<LibraryException>(ex);
    }

    [Fact]
    public void InsufficientStockException_PreservesMessage()
    {
        var ex = new InsufficientStockException("No stock");
        Assert.Equal("No stock", ex.Message);
    }

    // ---------- LoanRuleViolationException ----------

    [Fact]
    public void LoanRuleViolationException_InheritsFromLibraryException()
    {
        var ex = new LoanRuleViolationException("NMC", "limit exceeded");
        Assert.IsAssignableFrom<LibraryException>(ex);
    }

    [Fact]
    public void LoanRuleViolationException_SetsRuleName()
    {
        var ex = new LoanRuleViolationException("DELTA", "msg");
        Assert.Equal("DELTA", ex.RuleName);
    }

    [Fact]
    public void LoanRuleViolationException_PreservesMessage()
    {
        var ex = new LoanRuleViolationException("NCZ", "Ati atins limita maxima");
        Assert.Equal("Ati atins limita maxima", ex.Message);
    }

    [Theory]
    [InlineData("NMC")]
    [InlineData("DELTA")]
    [InlineData("NCZ")]
    [InlineData("PERSIMP")]
    public void LoanRuleViolationException_RuleName_IsStoredExactly(string ruleName)
    {
        var ex = new LoanRuleViolationException(ruleName, "m");
        Assert.Equal(ruleName, ex.RuleName);
    }

    // ---------- LibraryValidationException ----------

    [Fact]
    public void LibraryValidationException_InheritsFromLibraryException()
    {
        var ex = new LibraryValidationException(new[] { "e1" });
        Assert.IsAssignableFrom<LibraryException>(ex);
    }

    [Fact]
    public void LibraryValidationException_HasFixedBaseMessage()
    {
        var ex = new LibraryValidationException(new[] { "Eroare1" });
        Assert.Equal("Eroare de validare a datelor.", ex.Message);
    }

    [Fact]
    public void LibraryValidationException_StoresErrors()
    {
        var errors = new List<string> { "E1", "E2", "E3" };
        var ex = new LibraryValidationException(errors);

        // verificam ca lista e pastrata (referinta) sau cel putin continutul
        Assert.NotNull(ex.Errors);
        Assert.Equal(errors, ex.Errors);
    }

    [Fact]
    public void LibraryValidationException_ToString_ContainsBaseMessageAndErrors()
    {
        var ex = new LibraryValidationException(new[] { "A", "B" });

        var text = ex.ToString();

        Assert.Contains("Eroare de validare a datelor.", text);
        Assert.Contains("Detalii:", text);
        Assert.Contains("A", text);
        Assert.Contains("B", text);
    }

    [Fact]
    public void LibraryValidationException_ToString_WhenNoErrorsStillWorks()
    {
        var ex = new LibraryValidationException(Array.Empty<string>());

        var text = ex.ToString();

        Assert.Contains("Eroare de validare a datelor.", text);
        Assert.Contains("Detalii:", text);
    }
}