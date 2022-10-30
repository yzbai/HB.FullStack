using System;

namespace HB.FullStack.Database.DbModels
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DbModelAttribute : Attribute
    {
        public string? DbName { get; internal set; }

        public string? DbKind { get; internal set; }

        public string? TableName { get; set; }

        public bool? ReadOnly { get; set; }
    }
}