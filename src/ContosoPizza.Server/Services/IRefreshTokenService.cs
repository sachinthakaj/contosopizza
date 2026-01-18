namespace ContosoPizza.Services;

public interface IRefreshTokenService
{
    Task<string> CreateRefreshTokenAsync(int userId);
    Task<string> RotateRefreshTokenAsync(string oldToken, int userId);
    Task<bool> RevokeTokenAsync(string token);
}
