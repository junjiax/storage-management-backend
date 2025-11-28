using dotnet_backend.Data;
using dotnet_backend.DTOs.Auth;
using dotnet_backend.DTOs.Common;
using dotnet_backend.Models;
using dotnet_backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dotnet_backend.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly StoreDbContext _db;
		private readonly ITokenService _tokenService;

		public AuthController(StoreDbContext db, ITokenService tokenService)
		{
			_db = db;
			_tokenService = tokenService;
		}

		[HttpPost("register")]
		public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
			{
				return BadRequest(ApiResponse<AuthResponse>.Fail("Username and password are required", 400));
			}

			var exists = await _db.Users.AnyAsync(u => u.Username == request.Username);
			if (exists)
			{
				return Conflict(ApiResponse<AuthResponse>.Fail("Username already exists", 409));
			}

			var hashed = PasswordHasher.Hash(request.Password);
			var user = new User
			{
				Username = request.Username,
				Password = hashed,
				FullName = request.FullName,
				Role = string.IsNullOrWhiteSpace(request.Role) ? "staff" : request.Role
			};

			_db.Users.Add(user);
			await _db.SaveChangesAsync();

			var (token, expiresAt) = _tokenService.GenerateToken(user);
			return Ok(ApiResponse<AuthResponse>.Ok(new AuthResponse
			{
				Token = token,
				ExpiresAt = expiresAt,
				Username = user.Username,
				Role = user.Role
			}));
		}

		[HttpPost("login")]
		public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
		{
			var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
			if (user == null)
			{
				return Unauthorized(ApiResponse<AuthResponse>.Fail("Invalid username or password", 401));
			}

			var valid = PasswordHasher.Verify(request.Password, user.Password);
			if (!valid)
			{
				return Unauthorized(ApiResponse<AuthResponse>.Fail("Invalid username or password", 401));
			}

			var (token, expiresAt) = _tokenService.GenerateToken(user);
			return Ok(ApiResponse<AuthResponse>.Ok(new AuthResponse
			{
				Token = token,
				ExpiresAt = expiresAt,
				Username = user.Username,
				Role = user.Role,
				UserId = user.UserId
			},
				message: "Login successful",
				statusCode: 200
			));
		}
	}
}


