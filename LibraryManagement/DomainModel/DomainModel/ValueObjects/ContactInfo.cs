namespace DomainModel.ValueObjects;

// ContactInfo (pentru a forta regula: telefon sau email)
public record ContactInfo(string PhoneNumber, string Email);