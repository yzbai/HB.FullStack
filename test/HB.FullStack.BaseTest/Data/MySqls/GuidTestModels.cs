using System;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Data.MySqls
{
    [DbTable(DbSchema_Mysql)]
    public class User : TimestampGuidDbModel
    {
        public string Name { get; set; } = null!;

    }

    [DbTable(DbSchema_Mysql)]
    public class UserProfile : TimestampGuidDbModel
    {
        [DbForeignKey(typeof(User), true)]
        public Guid UserId { get; set; }

        public int Age { get; set; }
    }
}
