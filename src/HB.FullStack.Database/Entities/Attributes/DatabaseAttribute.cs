#nullable enable

using System;

namespace HB.FullStack.Database.Def
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DatabaseAttribute : Attribute
    {
        public string? DatabaseName { get; set; }

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