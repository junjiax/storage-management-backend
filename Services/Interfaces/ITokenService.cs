using dotnet_backend.Models;

namespace dotnet_backend.Services
{
	public interface ITokenService
	{
		(string token, DateTime expiresAt) GenerateToken(User user);
	}
}


