namespace DomainModel.Entities;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Inregistreaza imprumutul unei carti de catre un cititor.
/// </summary>
public class Loan
{
    [Key]
    public int Id { get; set; }

    public int ReaderId { get; set; }
    public virtual Reader Reader { get; set; }

    public int EditionId { get; set; }
    public virtual Edition Edition { get; set; }

    public DateTime LoanDate { get; set; }

    /// <summary>
    /// Data la care trebuie returnata cartea.
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Data efectiva a returnarii (null daca nu a fost returnata).
    /// </summary>
    public DateTime? ReturnDate { get; set; }

    /// <summary>
    /// Lista de prelungiri acordate pentru acest imprumut.
    /// </summary>
    public virtual ICollection<LoanExtension> Extensions { get; set; } = new List<LoanExtension>();
}