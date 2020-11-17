#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace HB.Framework.Database
{
    public class EntityInfo
    {
        [DisallowNull, NotNull]
        public string? EntityTypeFullName { get; set; }

        [DisallowNull, NotNull]
        public string? DatabaseName { get; set; }

        [DisallowNull, NotNull]
        public string? TableName { get; set; }
        
        public string? Description { get; set; }

        public bool ReadOnly { get; set; }

    }
}
