using ContosoPizza.Models;
using System.ComponentModel.DataAnnotations;

namespace ContosoPizza.DTOs.Users;

public sealed class UserCreateOrUpdateDto
{
    [Required]
    [StringLength(100)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    public static string? GetFirstValidationError(UserCreateOrUpdateDto? dto)
    {
        if (dto is null)
            return "Request body is required.";

        var results = new List<ValidationResult>();
        var context = new ValidationContext(dto);
        var isValid = Validator.TryValidateObject(dto, context, results, validateAllProperties: true);
        if (isValid)
            return null;

        return results.FirstOrDefault()?.ErrorMessage ?? "Validation failed.";
    }

    public static UserCreateOrUpdateDto From(User user) =>
        new()
        {
            UserName = user.UserName,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
        };

    public User ToEntity(int? id = null) =>
        new()
        {
            Id = id ?? 0,
            UserName = UserName,
            Email = Email,
            PasswordHash = PasswordHash,
        };
}