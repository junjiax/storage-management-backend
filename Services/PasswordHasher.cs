using System.Security.Cryptography;
using System.Text;

namespace dotnet_backend.Services
{
	public static class PasswordHasher
	{
		private const int SaltSize = 16; // 128 bit
		private const int KeySize = 32;  // 256 bit
		private const int Iterations = 100_000;
		private const char Delimiter = ':';

		public static string Hash(string password)
		{
			using var rng = RandomNumberGenerator.Create();
			var salt = new byte[SaltSize];
			rng.GetBytes(salt);

			var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
			return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(key), Iterations.ToString());
		}

		public static bool Verify(string password, string hash)
		{
			var parts = hash.Split(Delimiter);
			if (parts.Length != 3) return false;
			var salt = Convert.FromBase64String(parts[0]);
			var key = Convert.FromBase64String(parts[1]);
			var iterations = int.Parse(parts[2]);

			var computed = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, KeySize);
			return CryptographicOperations.FixedTimeEquals(computed, key);
		}
	}
}


