using ContosoPizza.DTOs.Users;
using ContosoPizza.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContosoPizza.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly UserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, UserService userService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] UserCreateDto dto)
    {
        var response = await _userService.CreateAsync(dto);
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (request is null)
            return BadRequest(new { message = "Request body is required" });

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var response = await _authService.AuthenticateAsync(request);
        if (response is null)
            return Unauthorized(new { message = "Invalid username or password" });

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("refreshToken", response.RefreshToken, cookieOptions);

        return Ok(new
        {
            accessToken = response.AccessToken,
            expiresAt = response.ExpiresAt,
            user = response.User
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Unauthorized(new { message = "Refresh token not found" });

        var response = await _authService.RefreshTokenAsync(refreshToken);
        if (response is null)
            return Unauthorized(new { message = "Invalid refresh token" });

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("refreshToken", response.RefreshToken, cookieOptions);

        return Ok(new
        {
            accessToken = response.AccessToken,
            expiresAt = response.ExpiresAt,
            user = response.User
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await _authService.RevokeTokenAsync(refreshToken);
        }

        Response.Cookies.Delete("refreshToken");

        _logger.LogInformation("User {User} logged out", User.Identity?.Name);

        return Ok(new { message = "Logged out successfully" });
    }

}