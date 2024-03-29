﻿using System;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Data.Sqlites
{
    [DbTable(DbSchema_Sqlite)]
    public class User : TimestampGuidDbModel
    {
        public string Name { get; set; } = null!;

    }

    [DbTable(DbSchema_Sqlite)]
    public class UserProfile : TimestampGuidDbModel
    {
        [DbForeignKey(typeof(User), true)]
        public Guid UserId { get; set; }

        public int Age { get; set; }
    }
}
