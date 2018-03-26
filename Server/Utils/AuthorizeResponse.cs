using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Server.Utils
{
    public class AuthorizeResponse
    {
        public string accessToken { get; }
        public string refreshToken { get; }
        public long userId { get; }

        public AuthorizeResponse(string accessToken, string refreshToken, long userId)
        {
            this.accessToken = accessToken;
            this.refreshToken = refreshToken;
            this.userId = userId;
        }
    }
}