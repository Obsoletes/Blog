using System;
using System.Collections.Generic;
using Blog.Extension;

namespace Blog.Service
{
    [Inject(Lifetime.Singleton)]
    public interface ITokenService
    {
        string GenerateRefreshToken();
        string GetToken(params (string Key,string Value)[] kvs);
        string Heartbeat(string token);
        bool TryGetUserIDFromExpiredToken(string token, out int userID);
    }
}
