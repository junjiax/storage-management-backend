using dotnet_backend.Data;
using dotnet_backend.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_backend.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly StoreDbContext _db;

		public UserRepository(StoreDbContext db)
		{
			_db = db;
		}

		public async Task<List<User>> GetAllAsync()
		{
			return await _db.Users.OrderBy(u => u.UserId).ToListAsync();
		}

		public async Task<User?> GetByIdAsync(int id)
		{
			return await _db.Users.FirstOrDefaultAsync(u => u.UserId == id);
		}

		public async Task<User?> GetByUsernameAsync(string username)
		{
			return await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
		}

		public async Task<bool> ExistsByUsernameAsync(string username)
		{
			return await _db.Users.AnyAsync(u => u.Username == username);
		}

		public async Task AddAsync(User user)
		{
			await _db.Users.AddAsync(user);
		}

		public Task DeleteAsync(User user)
		{
			_db.Users.Remove(user);
			return Task.CompletedTask;
		}

		public async Task SaveChangesAsync()
		{
			await _db.SaveChangesAsync();
		}
	}
}


