using ContosoPizza.Models;

namespace ContosoPizza.DTOs.Users;

public sealed class UserDto
{
    public int Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;

    public static UserDto From(User user) =>
        new()
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
        };
}