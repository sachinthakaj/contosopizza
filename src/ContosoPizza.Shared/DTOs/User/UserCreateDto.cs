using System.ComponentModel.DataAnnotations;

namespace ContosoPizza.DTOs.Users;

public sealed class UserCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255, MinimumLength = 3)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    public static string? GetFirstValidationError(UserCreateDto? dto)
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
}