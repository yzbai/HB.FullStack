using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.Def;

namespace HB.FullStack.Server.UserActivityTrace
{
    public class UserActivity : IdGenEntity
    {
        public UserActivity() { }

        public UserActivity(long? signInTokenId, long? userId, string? ip, string? url, string? arguments, int? resultStatusCode)
        {
            SignInTokenId = signInTokenId;
            UserId = userId;
            Ip = ip;
            Url = url;
            Arguments = arguments;
            ResultStatusCode = resultStatusCode;
        }

        [EntityProperty]
        public long? UserId { get; }

        [EntityProperty]
        public long? SignInTokenId { get; }

        [EntityProperty]
        public string? Ip { get; }

        [EntityProperty]
        public string? Url { get; }

        [EntityProperty]
        public string? Arguments { get; }

        [EntityProperty]
        public int? ResultStatusCode { get; }
    }
}
