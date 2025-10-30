using System.Text.Json.Serialization;

namespace dotnet_backend.DTOs.Common
{
	public class ApiResponse
	{
		[JsonPropertyName("success")]
		public bool Success { get; set; }

		[JsonPropertyName("message")]
		public string? Message { get; set; }

		[JsonPropertyName("statusCode")]
		public int StatusCode { get; set; }

		[JsonPropertyName("timestamp")]
		public DateTime Timestamp { get; set; } = DateTime.UtcNow;

		[JsonPropertyName("errors")]
		public IDictionary<string, string[]>? Errors { get; set; }

		public static ApiResponse Ok(string? message = null, int statusCode = 200)
		{
			return new ApiResponse { Success = true, Message = message, StatusCode = statusCode };
		}

		public static ApiResponse Fail(string? message, int statusCode, IDictionary<string, string[]>? errors = null)
		{
			return new ApiResponse { Success = false, Message = message, StatusCode = statusCode, Errors = errors };
		}
	}

	public class ApiResponse<T> : ApiResponse
	{
		[JsonPropertyName("data")]
		public T? Data { get; set; }

		public static ApiResponse<T> Ok(T data, string? message = null, int statusCode = 200)
		{
			return new ApiResponse<T> { Success = true, Message = message, StatusCode = statusCode, Data = data };
		}

		public new static ApiResponse<T> Fail(string? message, int statusCode, IDictionary<string, string[]>? errors = null)
		{
			return new ApiResponse<T> { Success = false, Message = message, StatusCode = statusCode, Errors = errors };
		}
	}
}


