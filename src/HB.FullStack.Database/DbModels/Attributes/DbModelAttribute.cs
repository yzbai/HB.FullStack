using System;

namespace HB.FullStack.Database.DbModels
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DbModelAttribute : Attribute
    {
        public DbSchema DbSchema { get; set; } = null!;

        public string? TableName { get; set; }

        public bool? ReadOnly { get; set; }

        public DbModelAttribute(DbSchema dbSchema)
        {
            DbSchema = dbSchema;
        }
    }
}