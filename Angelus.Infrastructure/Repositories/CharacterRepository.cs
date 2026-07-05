using Microsoft.EntityFrameworkCore;
using Angelus.Domain.Entities;
using Angelus.Domain.Interfaces;
using Angelus.Infrastructure.Data;

namespace Angelus.Infrastructure.Repositories;

public class CharacterRepository(AppDbContext db) : ICharacterRepository
{
    public Task<List<Character>> GetByUserIdAsync(Guid userId) =>
        db.Characters.Where(c => c.UserId == userId).ToListAsync();

    public Task<Character?> GetByIdAsync(Guid id) =>
        db.Characters.FirstOrDefaultAsync(c => c.Id == id);

    public Task<bool> ExistsByNameAsync(string name) =>
        db.Characters.AnyAsync(c => c.Name == name);

    public Task<bool> UserHasCharacterAsync(Guid userId) =>
        db.Characters.AnyAsync(c => c.UserId == userId);

    public async Task AddAsync(Character character)
    {
        db.Characters.Add(character);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Character character)
    {
        db.Characters.Remove(character);
        await db.SaveChangesAsync();
    }
}
