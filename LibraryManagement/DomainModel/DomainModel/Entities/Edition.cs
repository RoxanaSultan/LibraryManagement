namespace DomainModel.Entities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Reprezinta o editie specifica a unei carti.
/// </summary>
public class Edition
{
    [Key]
    public int Id { get; set; }

    public int BookId { get; set; }
    public virtual Book Book { get; set; }

    [Required]
    public string Publisher { get; set; }

    public int Year { get; set; }

    public string EditionNumber { get; set; }

    public int PageCount { get; set; }

    /// <summary>
    /// Tipul cartii (ex: brosata, hardcover).
    /// </summary>
    public string BookType { get; set; }

    /// <summary>
    /// Numarul total de exemplare achizitionate initial.
    /// </summary>
    public int InitialStock { get; set; }

    /// <summary>
    /// Cate exemplare sunt disponibile acum in biblioteca.
    /// </summary>
    public int CurrentStock { get; set; }

    /// <summary>
    /// Cate exemplare sunt rezervate EXCLUSIV pentru sala de lectura.
    /// </summary>
    public int ReadingRoomOnlyCount { get; set; }

    /// <summary>
    /// Indica daca toata editia este doar pentru sala de lectura.
    /// </summary>
    public bool IsEntirelyForReadingRoom => ReadingRoomOnlyCount >= InitialStock;
}