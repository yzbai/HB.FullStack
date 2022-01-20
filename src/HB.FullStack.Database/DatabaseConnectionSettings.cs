/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */


using System.Diagnostics.CodeAnalysis;

namespace HB.FullStack.Database
{
    public class DatabaseConnectionSettings
    {
        [DisallowNull, NotNull]
        public string DatabaseName { get; set; } = null!;

        [DisallowNull, NotNull]
        public string ConnectionString { get; set; } = null!;

        public bool IsMaster { get; set; } = true;
    }
}