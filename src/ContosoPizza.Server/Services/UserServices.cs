using ContosoPizza.DTOs.Users;
using ContosoPizza.Models;
using ContosoPizza.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ContosoPizza.Services;

public sealed class UserService
{
    private readonly UserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(UserRepository userRepository, IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<IReadOnlyList<UserDto>>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var dtos = users.Select(UserDto.From).ToList();

        return ApiResponse<IReadOnlyList<UserDto>>.Ok(
            dtos,
            message: "Users fetched successfully.",
            statusCode: StatusCodes.Status200OK
        );
    }

    public async Task<ApiResponse<UserDto>> GetByIdAsync(int id)
    {
        if (id <= 0)
            return ApiResponse<UserDto>.Fail("Invalid user id.", StatusCodes.Status400BadRequest);

        var user = await _userRepository.GetAsync(id);
        if (user is null)
            return ApiResponse<UserDto>.Fail("User not found.", StatusCodes.Status404NotFound);

        return ApiResponse<UserDto>.Ok(
            UserDto.From(user),
            message: "User fetched successfully.",
            statusCode: StatusCodes.Status200OK
        );
    }


    public async Task<ApiResponse<UserDto>> CreateAsync(UserCreateDto dto)
    {
        var validation = UserCreateDto.GetFirstValidationError(dto);
        if (validation is not null)
            return ApiResponse<UserDto>.Fail(validation, StatusCodes.Status400BadRequest);

        try
        {
            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                Role = dto.Role
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
            await _userRepository.AddAsync(user);

            return ApiResponse<UserDto>.Ok(
                UserDto.From(user),
                message: "User registered successfully.",
                statusCode: StatusCodes.Status201Created
            );
        }
        catch (DbUpdateException)
        {
            return ApiResponse<UserDto>.Fail(
                "Database rejected the new user.",
                StatusCodes.Status500InternalServerError
            );
        }
    }

    public async Task<ApiResponse<UserDto>> UpdateAsync(int id, UserCreateOrUpdateDto dto)
    {
        if (id <= 0)
            return ApiResponse<UserDto>.Fail("Invalid user id.", StatusCodes.Status400BadRequest);

        var validation = UserCreateOrUpdateDto.GetFirstValidationError(dto);
        if (validation is not null)
            return ApiResponse<UserDto>.Fail(validation, StatusCodes.Status400BadRequest);

        try
        {
            var user = dto.ToEntity(id);
            var updated = await _userRepository.UpdateAsync(user);

            if (!updated)
                return ApiResponse<UserDto>.Fail("User not found.", StatusCodes.Status404NotFound);

            var refreshed = await _userRepository.GetAsync(id);

            return ApiResponse<UserDto>.Ok(
                UserDto.From(refreshed!),
                message: "User updated successfully.",
                statusCode: StatusCodes.Status200OK
            );
        }
        
        catch (DbUpdateException)
        {
            return ApiResponse<UserDto>.Fail(
                "Database rejected the update.",
                StatusCodes.Status500InternalServerError
            );
        }
    }

    public async Task<ApiResponse<object?>> DeleteAsync(int id)
    {
        if (id <= 0)
            return ApiResponse<object?>.Fail("Invalid user id.", StatusCodes.Status400BadRequest);

        try
        {
            var deleted = await _userRepository.DeleteAsync(id);
            if (!deleted)
                return ApiResponse<object?>.Fail("User not found.", StatusCodes.Status404NotFound);

            return ApiResponse<object?>.Ok(
                data: null,
                message: "User deleted successfully.",
                statusCode: StatusCodes.Status200OK
            );
        }

        catch (DbUpdateException)
        {
            return ApiResponse<object?>.Fail(
                "Database rejected the delete operation.",
                StatusCodes.Status500InternalServerError
            );
        }
    }

}
