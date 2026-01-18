using System.ComponentModel.DataAnnotations;

namespace ContosoPizza.Models;

public class RefreshToken
{
    public int Id { get; set; }

    [MaxLength(512)]
    public string Token { get; set; } = string.Empty;

    public int UserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset ExpiresAt { get; set; }

    public bool IsUsed { get; set; }

    public DateTimeOffset? UsedAt { get; set; }

    public bool IsRevoked { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }
}
