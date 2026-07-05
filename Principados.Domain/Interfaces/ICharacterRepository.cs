using Principados.Domain.Entities;

namespace Principados.Domain.Interfaces;

public interface ICharacterRepository
{
    Task<List<Character>> GetByUserIdAsync(Guid userId);
    Task<Character?> GetByIdAsync(Guid id);
    Task<bool> ExistsByNameAsync(string name);
    Task<bool> UserHasCharacterAsync(Guid userId);
    Task AddAsync(Character character);
    Task DeleteAsync(Character character);
}
