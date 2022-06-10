

using System.Diagnostics.CodeAnalysis;

namespace HB.FullStack.Database
{
    public class EntitySetting
    {
        [DisallowNull, NotNull]
        public string EntityTypeFullName { get; set; } = null!;

        [DisallowNull, NotNull]
        public string DatabaseName { get; set; } = null!;

        [DisallowNull, NotNull]
        public string TableName { get; set; } = null!;

        public string? Description { get; set; }

        public bool ReadOnly { get; set; }
    }
}