using dotnet_backend.Models;

namespace dotnet_backend.Repositories
{
	public interface IUserRepository
	{
		Task<List<User>> GetAllAsync();
		Task<User?> GetByIdAsync(int id);
		Task<User?> GetByUsernameAsync(string username);
		Task<bool> ExistsByUsernameAsync(string username);
		Task AddAsync(User user);
		Task DeleteAsync(User user);
		Task SaveChangesAsync();
	}
}


