using System.Text.Json.Serialization;

namespace dotnet_backend.DTOs.Auth
{
	public class RegisterRequest
	{
		[JsonPropertyName("username")]
		public string Username { get; set; } = string.Empty;

		[JsonPropertyName("password")]
		public string Password { get; set; } = string.Empty;

		[JsonPropertyName("fullName")]
		public string? FullName { get; set; }

		[JsonPropertyName("role")]
		public string Role { get; set; } = "staff";
	}

	public class LoginRequest
	{
		[JsonPropertyName("username")]
		public string Username { get; set; } = string.Empty;

		[JsonPropertyName("password")]
		public string Password { get; set; } = string.Empty;
	}

	public class AuthResponse
	{
		[JsonPropertyName("userId")]
		public int UserId { get; set; }

		[JsonPropertyName("token")]
		public string Token { get; set; } = string.Empty;

		[JsonPropertyName("expiresAt")]
		public DateTime ExpiresAt { get; set; }

		[JsonPropertyName("username")]
		public string Username { get; set; } = string.Empty;

		[JsonPropertyName("role")]
		public string Role { get; set; } = string.Empty;
	}
}


