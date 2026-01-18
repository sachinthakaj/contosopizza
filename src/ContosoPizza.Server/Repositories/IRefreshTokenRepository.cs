using ContosoPizza.Models;

namespace ContosoPizza.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task SaveAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
    Task RevokeAllUserTokensAsync(int userId);
    Task<bool> RevokeTokenAsync(string token);
}
