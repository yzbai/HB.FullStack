#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace HB.FullStack.Database
{
    public class DatabaseConnectionSettings
    {
        [DisallowNull, NotNull]
        public string? DatabaseName { get; set; }

        [DisallowNull, NotNull]
        public string? ConnectionString { get; set; }

        public bool IsMaster { get; set; } = true;
    }
}