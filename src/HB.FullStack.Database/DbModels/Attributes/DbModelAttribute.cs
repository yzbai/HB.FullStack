using System;

namespace HB.FullStack.Database.DbModels
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DbModelAttribute : Attribute
    {
        public string? DbSchemaName { get; set; }

        public string? TableName { get; set; }

        public bool? ReadOnly { get; set; }

        public ConflictCheckMethods ConflictCheckMethods { get; set; } = ConflictCheckMethods.None;

        public DbModelAttribute()
        { }

        //public DbModelAttribute(ConflictCheckMethods conflictCheckMethods)
        //{
        //    ConflictCheckMethods = conflictCheckMethods;
        //}
    }
}