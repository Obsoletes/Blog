using System;

namespace Blog.Service.Implementation
{
	public class TokenService : ITokenService
	{
		public string GenerateRefreshToken()
		{
			return nameof(GenerateRefreshToken);
		}

		public string GetToken(params (string Key, string Value)[] kvs)
		{
			return nameof(GetToken);
		}

		public string Heartbeat(string token)
		{
			return nameof(Heartbeat);
		}

		public bool TryGetUserIDFromExpiredToken(string token, out int userID)
		{
			userID = 5;
			return true;
		}
	}
}
