namespace DomainModel.Entities;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Reprezinta autorul unei carti.
/// </summary>
public class Author
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; }

    /// <summary>
    /// Lista de carti scrise de acest autor.
    /// </summary>
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
