using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database;
using HB.FullStack.Database.Def;

namespace HB.FullStack.Server.UserActivityTrace
{
    public class UserActivity : IdGenEntity
    {
        public const int MAX_ARGUMENTS_LENGTH = 2000;
        public const int MAX_RESULT_ERROR_LENGTH = 1000;

        public UserActivity() { }

        public UserActivity(long? signInTokenId, long? userId, string? ip, string? url, string? httpMethod, string? arguments, int? resultStatusCode, string? resultType, ErrorCode? errorCode)
        {
            SignInTokenId = signInTokenId;
            UserId = userId;
            Ip = ip;
            Url = url;
            HttpMethod = httpMethod;
            Arguments = arguments;
            ResultStatusCode = resultStatusCode;
            ResultType = resultType;

            
            string? resultError = SerializeUtil.TryToJson(errorCode);

            if (resultError != null && resultError.Length > UserActivity.MAX_RESULT_ERROR_LENGTH)
            {
                resultError = resultError.Substring(0, UserActivity.MAX_RESULT_ERROR_LENGTH);
            }

            ResultError = resultError;
        }

        [EntityProperty]
        public long? UserId { get; set; }

        [EntityProperty]
        public long? SignInTokenId { get; set; }

        [EntityProperty]
        public string? Ip { get; set; }

        [EntityProperty]
        public string? Url { get; set; }

        [EntityProperty(MaxLength = 10)]
        public string? HttpMethod { get; set; }

        [EntityProperty(MaxLength = MAX_ARGUMENTS_LENGTH)]
        public string? Arguments { get; set; }

        [EntityProperty]
        public int? ResultStatusCode { get; set; }

        [EntityProperty]
        public string? ResultType { get; set; }
        
        [EntityProperty(MaxLength = MAX_RESULT_ERROR_LENGTH)]
        public string? ResultError { get; set; }
    }
}
