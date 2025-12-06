using dotnet_backend.DTOs.User;
using dotnet_backend.Models;
using dotnet_backend.Repositories;

namespace dotnet_backend.Services
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _users;

		public UserService(IUserRepository users)
		{
			_users = users;
		}

		public async Task<IEnumerable<UserResponse>> GetAllAsync()
		{
			var entities = await _users.GetAllAsync();
			return entities.Select(ToResponse);
		}

		public async Task<UserResponse?> GetByIdAsync(int id)
		{
			var user = await _users.GetByIdAsync(id);
			return user == null ? null : ToResponse(user);
		}

		public async Task<(UserResponse user, bool created, string? message)> CreateAsync(CreateUserRequest request)
		{
			if (await _users.ExistsByUsernameAsync(request.Username))
			{
				return (default!, false, "Username already exists");
			}

			var entity = new User
			{
				Username = request.Username,
				Password = PasswordHasher.Hash(request.Password),
				FullName = request.FullName,
				Role = string.IsNullOrWhiteSpace(request.Role) ? "staff" : request.Role
			};

			await _users.AddAsync(entity);
			await _users.SaveChangesAsync();
			return (ToResponse(entity), true, null);
		}

		public async Task<(UserResponse? user, string? message)> UpdateAsync(int id, UpdateUserRequest request)
		{
			var entity = await _users.GetByIdAsync(id);
			if (entity == null)
			{
				return (null, "User not found");
			}

			if (!string.IsNullOrWhiteSpace(request.FullName))
			{
				entity.FullName = request.FullName;
			}
			if (!string.IsNullOrWhiteSpace(request.Role))
			{
				entity.Role = request.Role!;
			}

			await _users.SaveChangesAsync();
			return (ToResponse(entity), null);
		}

		public async Task<(bool deleted, string? message)> DeleteAsync(int id)
		{
			var entity = await _users.GetByIdAsync(id);
			if (entity == null)
			{
				return (false, "User not found");
			}

			await _users.DeleteAsync(entity);
			await _users.SaveChangesAsync();
			return (true, null);
		}

		private static UserResponse ToResponse(User user)
		{
			return new UserResponse
			{
				UserId = user.UserId,
				Username = user.Username,
				FullName = user.FullName,
				Role = user.Role,
				CreatedAt = user.CreatedAt
			};
		}
	}
}


