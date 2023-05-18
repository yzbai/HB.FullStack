using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database.Config
{
    
    public class DbTableSchema
    {
        [DisallowNull, NotNull]
        public string TableName { get; set; } = null!;

        [DisallowNull, NotNull]
        public string DbModelFullName { get; set; } = null!;

        public bool? ReadOnly { get; set; }

        public ConflictCheckMethods? ConflictCheckMethods { get; set; }

        public IList<DbFieldSchema> Fields { get; set; } = new List<DbFieldSchema>();
    }
}