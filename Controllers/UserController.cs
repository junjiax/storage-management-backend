using dotnet_backend.Data;
using dotnet_backend.DTOs.Common;
using dotnet_backend.DTOs.User;
using dotnet_backend.Models;
using dotnet_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dotnet_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
public class UsersController : ControllerBase
	{
	private readonly IUserService _users;

	public UsersController(IUserService users)
		{
		_users = users;
		}

		[HttpGet]
		public async Task<ActionResult<ApiResponse<IEnumerable<UserResponse>>>> GetAll()
		{
		var users = await _users.GetAllAsync();
		return Ok(ApiResponse<IEnumerable<UserResponse>>.Ok(users));
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<ApiResponse<UserResponse>>> GetById([FromRoute] int id)
		{
		var user = await _users.GetByIdAsync(id);
		if (user == null)
			{
				return NotFound(ApiResponse<UserResponse>.Fail("User not found", 404));
			}
		return Ok(ApiResponse<UserResponse>.Ok(user));
		}

		[HttpPost]
		public async Task<ActionResult<ApiResponse<UserResponse>>> Create([FromBody] CreateUserRequest request)
		{
		if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
		{
			return BadRequest(ApiResponse<UserResponse>.Fail("Username and password are required", 400));
		}

		var (createdUser, created, message) = await _users.CreateAsync(request);
		if (!created)
		{
			return Conflict(ApiResponse<UserResponse>.Fail(message ?? "Cannot create user", 409));
		}

		return CreatedAtAction(nameof(GetById), new { id = createdUser.UserId }, ApiResponse<UserResponse>.Ok(createdUser, "User created", 201));
		}

		[HttpPut("{id:int}")]
		public async Task<ActionResult<ApiResponse<UserResponse>>> Update([FromRoute] int id, [FromBody] UpdateUserRequest request)
		{
		var (user, error) = await _users.UpdateAsync(id, request);
		if (user == null)
		{
			return NotFound(ApiResponse<UserResponse>.Fail(error ?? "User not found", 404));
		}
		return Ok(ApiResponse<UserResponse>.Ok(user, "User updated"));
		}

		[HttpDelete("{id:int}")]
		public async Task<ActionResult<ApiResponse>> Delete([FromRoute] int id)
		{
		var (deleted, error) = await _users.DeleteAsync(id);
		if (!deleted)
		{
			return NotFound(ApiResponse.Fail(error ?? "User not found", 404));
		}
		return Ok(ApiResponse.Ok("User deleted"));
		}

	}
}


