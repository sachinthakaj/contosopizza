using ContosoPizza.DTOs.Users;

public class AuthResponse
{
    private readonly UserDto _user;
    public AuthResponse(UserDto user)
    {
        _user = user;
    }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = new();
}