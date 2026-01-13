using DomainModel.Entities;

namespace DomainModel.Interfaces;

public interface IDomainRepository
{
    Task<Domain> GetByIdAsync(int id);
    Task<IEnumerable<Domain>> GetAllAsync();

    /// <summary>
    /// Returneaza ierarhia completa de stramosi pentru un domeniu.
    /// </summary>
    Task<IEnumerable<Domain>> GetAncestorsAsync(int domainId);
}