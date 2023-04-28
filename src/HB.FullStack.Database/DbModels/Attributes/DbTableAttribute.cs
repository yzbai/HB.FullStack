using System;

namespace HB.FullStack.Database.DbModels
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DbTableAttribute : Attribute
    {
        public string DbSchemaName { get; set; } = null!;

        public string? TableName { get; set; }

        public bool? ReadOnly { get; set; }

        public DbTableAttribute(string dbSchemaName)
        {
            DbSchemaName = dbSchemaName;
        }
    }
}