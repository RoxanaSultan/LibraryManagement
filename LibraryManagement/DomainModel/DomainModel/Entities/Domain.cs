namespace DomainModel.Entities;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Reprezinta un domeniu sau subdomeniu de carti.
/// </summary>
public class Domain
{
    /// <summary>
    /// Primeste sau seteaza identificatorul unic.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Primeste sau seteaza numele domeniului (ex: Informatica).
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    /// <summary>
    /// Primeste sau seteaza ID-ul domeniului parinte.
    /// </summary>
    public int? ParentDomainId { get; set; }

    /// <summary>
    /// Navigare catre domeniul parinte (stramos).
    /// </summary>
    public virtual Domain ParentDomain { get; set; }

    /// <summary>
    /// Lista de subdomenii (descendenti directi).
    /// </summary>
    public virtual ICollection<Domain> SubDomains { get; set; } = new List<Domain>();

    /// <summary>
    /// Cartile care fac parte explicit din acest domeniu.
    /// </summary>
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();

    public IEnumerable<Domain> GetAllAncestors()
    {
        var ancestors = new List<Domain>();
        var current = this.ParentDomain;
        while (current != null)
        {
            ancestors.Add(current);
            current = current.ParentDomain;
        }
        return ancestors;
    }
}