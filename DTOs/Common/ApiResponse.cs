using System.Text.Json.Serialization;

namespace dotnet_backend.DTOs.Common
{
	public class ApiResponse
	{
		[JsonPropertyName("success")]
		[JsonPropertyOrder(0)]
		public bool Success { get; set; }

		[JsonPropertyName("message")]
		[JsonPropertyOrder(1)]
		public string? Message { get; set; }

		[JsonPropertyName("statusCode")]
		[JsonPropertyOrder(2)]
		public int StatusCode { get; set; }

		[JsonPropertyName("timestamp")]
		[JsonPropertyOrder(3)]
		public DateTime Timestamp { get; set; } = DateTime.UtcNow;

		[JsonPropertyName("errors")]
		[JsonPropertyOrder(4)]
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


		public static ApiResponse<T> Ok(T data, string? message = null, int statusCode = 200)
		{
			return new ApiResponse<T> { Data = data,Success = true, Message = message, StatusCode = statusCode };
		}

		public new static ApiResponse<T> Fail(string? message, int statusCode, IDictionary<string, string[]>? errors = null)
		{
			return new ApiResponse<T> { Success = false, Message = message, StatusCode = statusCode, Errors = errors };
		}
		[JsonPropertyName("data")]
		[JsonPropertyOrder(100)]
		public T? Data { get; set; }
	}
}


