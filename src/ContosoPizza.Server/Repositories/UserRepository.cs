using ContosoPizza.Models;
using ContosoPizza.Data;
using Microsoft.EntityFrameworkCore;

namespace ContosoPizza.Repositories;

public class UserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _db.Users.AsNoTracking().OrderBy(u => u.Id).ToListAsync();
    }
    public async Task<User?> GetAsync(int id)
    {
        return await _db.Users.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(User user)
    {
        var existing = await _db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        if (existing is null)
        return false;

        existing.UserName = user.UserName;
        existing.Email = user.Email;
        existing.PasswordHash = user.PasswordHash;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if(user is null)
        return false;

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return true;
    }
}