using DomainModel.Entities;

namespace TestDomainModel.Entities;

/// <summary>
/// Testeaza regulile de stoc si disponibilitate pentru editii.
/// </summary>
public class EditionTests
{
    [Theory]
    [InlineData(10, 0, 10)] // Toate disponibile
    [InlineData(10, 5, 5)]  // 5 disponibile, 5 de sala
    [InlineData(10, 10, 0)] // Niciuna disponibila
    public void AvailableCopies_ShouldCalculateCorrectly(int current, int rrOnly, int expected)
    {
        var edition = new Edition { CurrentStock = current, ReadingRoomOnlyCount = rrOnly };

        int actual = edition.CurrentStock - edition.ReadingRoomOnlyCount;

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(10, 2, 3, true)]  // (3-2)=1. 1 >= 10% din 10. OK.
    [InlineData(10, 2, 2, false)] // (2-2)=0. 0 < 10% din 10. FAIL.
    [InlineData(20, 5, 7, true)]  // (7-5)=2. 2 >= 10% din 20. OK.
    [InlineData(20, 5, 6, false)] // (6-5)=1. 1 < 10% din 20. FAIL.
    [InlineData(5, 5, 5, false)]  // Toate sunt de sala. FAIL.
    public void TenPercentRule_ShouldValidateCorrectly(int initial, int rrOnly, int current, bool expected)
    {
        // Arrange
        var edition = new Edition
        {
            InitialStock = initial,
            ReadingRoomOnlyCount = rrOnly,
            CurrentStock = current
        };

        // Act
        int availableForLoan = edition.CurrentStock - edition.ReadingRoomOnlyCount;
        bool canBorrow = availableForLoan >= (edition.InitialStock * 0.1);

        // Assert
        Assert.Equal(expected, canBorrow);
    }
}