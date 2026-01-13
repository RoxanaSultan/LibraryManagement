namespace DomainModel.Entities;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Reprezinta o carte din biblioteca.
/// </summary>
public class Book
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; }

    /// <summary>
    /// O carte poate avea mai multi autori.
    /// </summary>
    public virtual ICollection<Author> Authors { get; set; } = new List<Author>();

    /// <summary>
    /// Domeniile in care a fost incadrata explicit cartea.
    /// </summary>
    public virtual ICollection<Domain> ExplicitDomains { get; set; } = new List<Domain>();

    /// <summary>
    /// Editiile disponibile pentru aceasta carte.
    /// </summary>
    public virtual ICollection<Edition> Editions { get; set; } = new List<Edition>();

    public bool HasOverlappingDomains()
    {
        foreach (var d1 in ExplicitDomains)
        {
            foreach (var d2 in ExplicitDomains)
            {
                if (d1.Id == d2.Id) continue;
                // Daca unul este stramosul celuilalt, avem eroare conform cerintei
                if (d1.GetAllAncestors().Any(a => a.Id == d2.Id)) return true;
            }
        }
        return false;
    }
}
