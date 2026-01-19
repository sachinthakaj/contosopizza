using ContosoPizza.DTOs.Users;
using ContosoPizza.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ContosoPizza.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UserController : ControllerBase
{
	private readonly UserService _userService;

	public UserController(UserService userService)
	{
		_userService = userService;
	}

	[HttpGet]
	[ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserDto>>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserDto>>), StatusCodes.Status500InternalServerError)]
    [Authorize]
	public async Task<IActionResult> GetAll()
	{
		var response = await _userService.GetAllAsync();
		return StatusCode(response.StatusCode, response);
	}

	[HttpGet("{id:int}")]
	[ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> GetById(int id)
	{
		var response = await _userService.GetByIdAsync(id);
		return StatusCode(response.StatusCode, response);
	}


	[HttpPut("{id:int}")]
	[ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> Update(int id, [FromBody] UserCreateOrUpdateDto dto)
	{
		var response = await _userService.UpdateAsync(id, dto);
		return StatusCode(response.StatusCode, response);
	}

	[HttpDelete("{id:int}")]
	[ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status500InternalServerError)]
	public async Task<IActionResult> Delete(int id)
	{
		var response = await _userService.DeleteAsync(id);
		return StatusCode(response.StatusCode, response);
	}
}
