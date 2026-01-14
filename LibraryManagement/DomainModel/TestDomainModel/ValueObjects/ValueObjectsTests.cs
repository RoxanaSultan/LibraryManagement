using DomainModel.ValueObjects;

namespace TestDomainModel.ValueObjects;

public class ValueObjectsTests
{
    // ---------------- Address ----------------

    [Fact]
    public void Address_WithSameValues_AreEqual()
    {
        var a1 = new Address("Str. Lunga 1", "Brasov", "500000");
        var a2 = new Address("Str. Lunga 1", "Brasov", "500000");

        Assert.Equal(a1, a2);
        Assert.True(a1 == a2);
    }

    [Fact]
    public void Address_WithDifferentStreet_AreNotEqual()
    {
        var a1 = new Address("Str. Lunga 1", "Brasov", "500000");
        var a2 = new Address("Str. Lunga 2", "Brasov", "500000");

        Assert.NotEqual(a1, a2);
        Assert.True(a1 != a2);
    }

    [Fact]
    public void Address_HashCode_SameForEqualObjects()
    {
        var a1 = new Address("Str. Lunga 1", "Brasov", "500000");
        var a2 = new Address("Str. Lunga 1", "Brasov", "500000");

        Assert.Equal(a1.GetHashCode(), a2.GetHashCode());
    }

    [Fact]
    public void Address_WithExpression_CreatesNewInstance()
    {
        var original = new Address("Str. Lunga 1", "Brasov", "500000");

        var updated = original with { Street = "Str. Lunga 99" };

        Assert.NotSame(original, updated);
        Assert.Equal("Str. Lunga 1", original.Street);      // original neschimbat
        Assert.Equal("Str. Lunga 99", updated.Street);
        Assert.Equal(original.City, updated.City);
        Assert.Equal(original.ZipCode, updated.ZipCode);
    }

    [Fact]
    public void Address_Deconstruct_Works()
    {
        var address = new Address("Str. Lunga 1", "Brasov", "500000");

        var (street, city, zip) = address;

        Assert.Equal("Str. Lunga 1", street);
        Assert.Equal("Brasov", city);
        Assert.Equal("500000", zip);
    }

    [Fact]
    public void Address_ToString_ContainsTypeAndValues()
    {
        var address = new Address("Str. Lunga 1", "Brasov", "500000");

        var text = address.ToString();

        Assert.Contains("Address", text);
        Assert.Contains("Str. Lunga 1", text);
        Assert.Contains("Brasov", text);
        Assert.Contains("500000", text);
    }

    // ---------------- ContactInfo ----------------

    [Fact]
    public void ContactInfo_WithSameValues_AreEqual()
    {
        var c1 = new ContactInfo("0712345678", "ana@test.com");
        var c2 = new ContactInfo("0712345678", "ana@test.com");

        Assert.Equal(c1, c2);
        Assert.True(c1 == c2);
    }

    [Fact]
    public void ContactInfo_WithDifferentPhone_AreNotEqual()
    {
        var c1 = new ContactInfo("0712345678", "ana@test.com");
        var c2 = new ContactInfo("0799999999", "ana@test.com");

        Assert.NotEqual(c1, c2);
        Assert.True(c1 != c2);
    }

    [Fact]
    public void ContactInfo_HashCode_SameForEqualObjects()
    {
        var c1 = new ContactInfo("0712345678", "ana@test.com");
        var c2 = new ContactInfo("0712345678", "ana@test.com");

        Assert.Equal(c1.GetHashCode(), c2.GetHashCode());
    }

    [Fact]
    public void ContactInfo_WithExpression_CreatesNewInstance()
    {
        var original = new ContactInfo("0712345678", "ana@test.com");

        var updated = original with { Email = "roxana@endava.com" };

        Assert.NotSame(original, updated);
        Assert.Equal("ana@test.com", original.Email);
        Assert.Equal("roxana@endava.com", updated.Email);
        Assert.Equal(original.PhoneNumber, updated.PhoneNumber);
    }

    [Fact]
    public void ContactInfo_Deconstruct_Works()
    {
        var contact = new ContactInfo("0712345678", "ana@test.com");

        var (phone, email) = contact;

        Assert.Equal("0712345678", phone);
        Assert.Equal("ana@test.com", email);
    }

    [Fact]
    public void ContactInfo_ToString_ContainsTypeAndValues()
    {
        var contact = new ContactInfo("0712345678", "ana@test.com");

        var text = contact.ToString();

        Assert.Contains("ContactInfo", text);
        Assert.Contains("0712345678", text);
        Assert.Contains("ana@test.com", text);
    }

    // ---------------- Cross tests / edge-ish ----------------

    [Fact]
    public void Records_AreReferenceDifferentButValueEqual()
    {
        var a1 = new Address("S", "C", "Z");
        var a2 = new Address("S", "C", "Z");

        Assert.NotSame(a1, a2);     // instanțe diferite
        Assert.Equal(a1, a2);       // valoare egală
    }

    [Fact]
    public void Record_AllowsNulls_ByDefault()
    {
        // NOTE: record-urile nu valideaza automat null.
        // Testul e util ca sa fie clar comportamentul (si iti creste count).
        var address = new Address(null!, null!, null!);
        Assert.Null(address.Street);
        Assert.Null(address.City);
        Assert.Null(address.ZipCode);

        var contact = new ContactInfo(null!, null!);
        Assert.Null(contact.PhoneNumber);
        Assert.Null(contact.Email);
    }
}