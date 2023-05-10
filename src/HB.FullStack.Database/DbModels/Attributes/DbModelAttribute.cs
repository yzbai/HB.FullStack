using System;

namespace HB.FullStack.Database.DbModels
{


    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DbModelAttribute : Attribute
    {
        public string DbSchemaName { get; set; } = null!;

        public string? TableName { get; set; }

        public bool? ReadOnly { get; set; }

        public DbConflictCheckMethod ConflictCheckMethod { get; set; } = DbConflictCheckMethod.Both;

        public DbModelAttribute(string dbSchemaName)
        {
            DbSchemaName = dbSchemaName;
        }
    }
}