using System;

namespace HB.FullStack.Database.DbModels
{


    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DbModelAttribute : Attribute
    {
        public string DbSchemaName { get; set; } = null!;

        public string? TableName { get; set; }

        public bool? ReadOnly { get; set; }

        public ConflictCheckMethods ConflictCheckMethods { get; set; } = ConflictCheckMethods.OldNewValueCompare | ConflictCheckMethods.Timestamp;

        public DbModelAttribute(string dbSchemaName)
        {
            DbSchemaName = dbSchemaName;
        }
    }
}