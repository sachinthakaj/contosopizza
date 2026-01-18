using ContosoPizza.Data;
using ContosoPizza.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoPizza.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _db;

    public RefreshTokenRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<RefreshToken?> GetByTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult<RefreshToken?>(null);

        return _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task SaveAsync(RefreshToken refreshToken)
    {
        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        _db.RefreshTokens.Update(refreshToken);
        await _db.SaveChangesAsync();
    }

    public async Task RevokeAllUserTokensAsync(int userId)
    {
        var tokens = await _db.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync();

        if (tokens.Count == 0)
            return;

        var now = DateTimeOffset.UtcNow;
        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = now;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var existing = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
        if (existing is null)
            return false;

        if (existing.IsRevoked)
            return true;

        existing.IsRevoked = true;
        existing.RevokedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }
}
