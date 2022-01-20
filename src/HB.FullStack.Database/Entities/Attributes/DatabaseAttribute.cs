

using System;

namespace HB.FullStack.Database.Entities
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DatabaseAttribute : Attribute
    {
        public string? DatabaseName { get; internal set; }

        public string? TableName { get; set; }

        public string? Description { get; set; }

        public bool ReadOnly { get; set; }

        public string SuffixToRemove { get; set; } = "Entity";

        public DatabaseAttribute()
        {

        }

        public DatabaseAttribute(string databaseName)
        {
            DatabaseName = databaseName;
        }
    }
}