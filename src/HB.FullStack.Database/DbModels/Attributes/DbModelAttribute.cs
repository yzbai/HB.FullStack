using System;

namespace HB.FullStack.Database.DbModels
{


    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DbModelAttribute : Attribute
    {
        public string DbSchemaName { get; set; } = null!;

        public string? TableName { get; set; }

        public bool? ReadOnly { get; set; }

        public DbConflictCheckMethods ConflictCheckMethods { get; set; } = DbConflictCheckMethods.OldNewValueCompare | DbConflictCheckMethods.Timestamp;

        public DbModelAttribute(string dbSchemaName)
        {
            DbSchemaName = dbSchemaName;
        }
    }
}