using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Shared
{
    public static partial class Conventions
    {
        public static string GetRandomNickName(string prefix = "User")
        {
            return prefix + TimeUtil.UtcNowUnixTimeMilliseconds;
        }

        public static bool IsARandomNickName(string nickName)
        {
            return nickName.IsNotNullOrEmpty() && RandomNickNameRegex().IsMatch(nickName);
        }

        public static string GetLastUser(Guid? userId, string? mobile, string? clientId)
        {
            return $"u:{userId}-m:{mobile}-d:{clientId}";
        }

        [GeneratedRegex("^User{1}\\d{4,}$")]
        private static partial Regex RandomNickNameRegex();
    }
}
