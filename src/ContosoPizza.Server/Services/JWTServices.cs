using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ContosoPizza.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ContosoPizza.Services;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IOptions<JwtSettings> jwtSettings, ILogger<JwtService> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public string GenerateAccessToken(User user)
    {
        if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
        {
            throw new InvalidOperationException("JWT SecretKey is not configured. Set Jwt:SecretKey in configuration.");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        _logger.LogInformation($"Access token generated for user {user.UserName}");

        return tokenString;
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? ValidateToken(string token, bool validateLifetime = true)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(_jwtSettings.SecretKey))
        {
            _logger.LogWarning("Token validation failed: JWT SecretKey is not configured");
            return null;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = validateLifetime,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken)
            {
                _logger.LogWarning("Token validation failed: validated token is not a JWT");
                return null;
            }

            var alg = jwtToken.Header.Alg;
            var isHmacSha256 =
                string.Equals(alg, SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase)
                || string.Equals(alg, SecurityAlgorithms.HmacSha256Signature, StringComparison.OrdinalIgnoreCase);

            if (!isHmacSha256)
            {
                _logger.LogWarning($"Token validation failed: invalid algorithm {alg}");
                return null;
            }

            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Token validation failed: {ex.Message}");
        }

        return null;
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        return ValidateToken(token, validateLifetime: false);
    }
}