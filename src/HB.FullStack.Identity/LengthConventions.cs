using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Server.Identity
{
    public static class LengthConventions
    {
        public const int MAX_URL_LENGTH = 2000;
        public const int MAX_ARGUMENTS_LENGTH = 2000;
        public const int MAX_RESULT_ERROR_LENGTH = 2000;

        public const int MAX_USER_CLAIM_VALUE_LENGTH = 5000;
        public const int MAX_ROLE_COMMENT_LENGTH = 1024;

        public const int MAX_USER_LOGIN_NAME_LENGTH = 100;
        public const int MAX_USER_MOBILE_LENGTH = 14;
        public const int MAX_USER_EMAIL_LENGTH = 256;

        public const int MAX_FILE_NAME_LENGTH = 128;
    }
}
