namespace DomainModel.Entities;

using System;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Reprezinta o prelungire a termenului de returnare pentru un imprumut.
/// </summary>
public class LoanExtension
{
    [Key]
    public int Id { get; set; }

    public int LoanId { get; set; }
    public virtual Loan Loan { get; set; }

    /// <summary>
    /// Data la care s-a solicitat prelungirea.
    /// </summary>
    public DateTime ExtensionDate { get; set; }

    /// <summary>
    /// Cate zile s-au adaugat la termenul initial (folosit pentru calculul limitei LIM).
    /// </summary>
    public int DaysAdded { get; set; }
}