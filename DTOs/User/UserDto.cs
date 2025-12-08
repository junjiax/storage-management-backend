using System.Text.Json.Serialization;

namespace dotnet_backend.DTOs.User
{
    public class CreateUserRequest
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

    public class UpdateUserRequest
    {
        [JsonPropertyName("fullName")]
        public string? FullName { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }
    }

    public class UserResponse
    {
        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("fullName")]
        public string? FullName { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}