using dotnet_backend.DTOs.User;

namespace dotnet_backend.Services
{
	public interface IUserService
	{
		Task<IEnumerable<UserResponse>> GetAllAsync();
		Task<UserResponse?> GetByIdAsync(int id);
		Task<(UserResponse user, bool created, string? message)> CreateAsync(CreateUserRequest request);
		Task<(UserResponse? user, string? message)> UpdateAsync(int id, UpdateUserRequest request);
		Task<(bool deleted, string? message)> DeleteAsync(int id);
	}
}


