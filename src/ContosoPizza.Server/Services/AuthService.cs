using ContosoPizza.DTOs.Users;
using ContosoPizza.Models;
using ContosoPizza.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ContosoPizza.Services;

public interface IAuthService
{
    Task<AuthResponse?> AuthenticateAsync(LoginRequestDto request);
    Task<AuthResponse?> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
}

public sealed class AuthService : IAuthService
{
    private readonly IJwtService _jwtService;
    private readonly UserRepository _userRepository;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly JwtSettings _jwtSettings;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IJwtService jwtService,
        UserRepository userRepository,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IOptions<JwtSettings> jwtSettings,
        IPasswordHasher<User> passwordHasher,
        ILogger<AuthService> logger)
    {
        _jwtService = jwtService;
        _userRepository = userRepository;
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtSettings = jwtSettings.Value;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<AuthResponse?> AuthenticateAsync(LoginRequestDto request)
    {
        if (request is null)
            return null;

        var userName = request.Username?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(request.Password))
            return null;

        var user = await _userRepository.GetByUserNameAsync(userName);
        if (user is null)
        {
            _logger.LogWarning($"Authentication failed: invalid password for username {userName}");
            return null;
        }

        var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning($"Authentication failed: invalid password for username {userName}");
            return null;
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user.Id);

        _logger.LogInformation($"User {user.UserName} authenticated successfully");

        var userDto = UserDto.From(user);

        var response = new AuthResponse(userDto)
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            User = userDto,
        };

        return response;
    }

    public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        var existing = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
        if (existing is null)
            return null;

        if (existing.IsRevoked || existing.ExpiresAt <= DateTimeOffset.UtcNow)
            return null;

        var user = await _userRepository.GetAsync(existing.UserId);
        if (user is null)
            return null;

        string newRefreshToken;
        try
        {
            newRefreshToken = await _refreshTokenService.RotateRefreshTokenAsync(refreshToken, user.Id);
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning($"Refresh token rotation failed for user {user.Id}: {ex.Message}");
            return null;
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var userDto = UserDto.From(user);

        return new AuthResponse(userDto)
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            User = userDto,
        };
    }

    public Task<bool> RevokeTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Task.FromResult(false);

        return _refreshTokenService.RevokeTokenAsync(refreshToken);
    }
}