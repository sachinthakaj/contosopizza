using System.Security.Cryptography;
using ContosoPizza.Models;
using ContosoPizza.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace ContosoPizza.Services;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _repository;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(IRefreshTokenRepository repository, ILogger<RefreshTokenService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<string> CreateRefreshTokenAsync(int userId)
    {
        var newToken = GenerateSecureToken();

        await _repository.SaveAsync(new RefreshToken
        {
            Token = newToken,
            UserId = userId,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedAt = DateTimeOffset.UtcNow
        });

        return newToken;
    }

    // Integrated from your snippet (token rotation + reuse detection)
    public async Task<string> RotateRefreshTokenAsync(string oldToken, int userId)
    {
        var existingToken = await _repository.GetByTokenAsync(oldToken);

        if (existingToken == null || existingToken.UserId != userId)
        {
            _logger.LogWarning($"Invalid refresh token rotation attempt for user {userId}");
            throw new SecurityTokenException("Invalid refresh token");
        }

        if (existingToken.IsRevoked)
        {
            _logger.LogWarning($"Refresh token revoked for user {userId}");
            throw new SecurityTokenException("Invalid refresh token");
        }

        if (existingToken.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _logger.LogWarning($"Refresh token expired for user {userId}");
            throw new SecurityTokenException("Refresh token expired");
        }

        if (existingToken.IsUsed)
        {
            _logger.LogWarning($"Refresh token reuse detected for user {userId}. Revoking all tokens.");
            await _repository.RevokeAllUserTokensAsync(userId);
            throw new SecurityTokenException("Token reuse detected. Please log in again.");
        }

        existingToken.IsUsed = true;
        existingToken.UsedAt = DateTimeOffset.UtcNow;
        await _repository.UpdateAsync(existingToken);

        var newToken = GenerateSecureToken();
        await _repository.SaveAsync(new RefreshToken
        {
            Token = newToken,
            UserId = userId,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            CreatedAt = DateTimeOffset.UtcNow
        });

        return newToken;
    }

    public Task<bool> RevokeTokenAsync(string token) => _repository.RevokeTokenAsync(token);

    private static string GenerateSecureToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
