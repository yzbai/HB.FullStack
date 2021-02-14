#nullable enable

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