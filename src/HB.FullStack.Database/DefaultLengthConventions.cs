/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

namespace HB.FullStack.Database
{
    public static class DefaultLengthConventions
    {
        public const int MAX_LAST_USER_LENGTH = 100;

        public const int MAX_VARCHAR_LENGTH = 16379;

        public const int DEFAULT_VARCHAR_LENGTH = 200;

        public const int MAX_MEDIUM_TEXT_LENGTH = 4194303;

        /// <summary>
        /// mysql每一行最大的字节数
        /// </summary>
        public const int MYSQL_MAX_ROW_BYTES = 65535;

        /// <summary>
        /// mysql最大字节数除以4，最大字符数
        /// </summary>
        public const int MYSQL_MAX_ROW_LENGTH = 16383; // 65535/4 because of utfmb4
    }
}