using System;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.DatabaseTests.Data.MySqls
{
    [DbModel(DbSchema_Mysql)]
    public class User : TimestampGuidDbModel
    {
        public string Name { get; set; } = null!;

    }

    [DbModel(DbSchema_Mysql)]
    public class UserProfile : TimestampGuidDbModel
    {
        [ForeignKey(typeof(User), true)]
        public Guid UserId { get; set; }

        public int Age { get; set; }
    }
}
