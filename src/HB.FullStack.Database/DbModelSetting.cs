
using System.Diagnostics.CodeAnalysis;

namespace HB.FullStack.Database
{
    public class DbModelSetting
    {
        [DisallowNull, NotNull]
        public string ModelTypeFullName { get; set; } = null!;

        [DisallowNull, NotNull]
        public string DatabaseName { get; set; } = null!;

        [DisallowNull, NotNull]
        public string TableName { get; set; } = null!;

        public string? Description { get; set; }

        public bool ReadOnly { get; set; }
    }
}