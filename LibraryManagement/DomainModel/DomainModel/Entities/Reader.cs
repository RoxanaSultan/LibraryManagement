namespace DomainModel.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Reprezinta un utilizator al bibliotecii.
/// </summary>
public class Reader
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    public string Address { get; set; }

    // Date de contact
    public string PhoneNumber { get; set; }
    public string Email { get; set; }

    /// <summary>
    /// Indica daca cititorul este si personal al bibliotecii (pentru regulile de praguri).
    /// </summary>
    public bool IsLibraryStaff { get; set; }

    /// <summary>
    /// Contul unic pentru a identifica daca mai multi cititori partajeaza acelasi cont (Regula pag. 2).
    /// </summary>
    public string AccountId { get; set; }
}
