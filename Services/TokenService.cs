using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using dotnet_backend.Models;
using Microsoft.AspNetCore.Components.Sections;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace dotnet_backend.Services
{
	public class TokenService : ITokenService
	{
		private readonly IConfiguration _configuration;

		public TokenService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public (string token, DateTime expiresAt) GenerateToken(User user)
		{
			var jwtSection = _configuration.GetSection("Jwt");
			var issuer = jwtSection["Issuer"];
			var audience = jwtSection["Audience"];
			var key = jwtSection["Key"];
			Console.WriteLine(">>>>>>>>>>>>>>JWT Key: " + key);
         var expiresMinutes = int.TryParse(jwtSection["ExpiresMinutes"], out var m) ? m : 60;

			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			var claims = new List<Claim>
			{
				new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
				new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
				new Claim(ClaimTypes.Role, user.Role)
			};

			var expiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes);

			var token = new JwtSecurityToken(
				issuer: issuer,
				audience: audience,
				claims: claims,
				expires: expiresAt,
				signingCredentials: credentials
			);

			var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
			return (tokenString, expiresAt);
		}
	}
}


