/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Data.MySqls
{
    [DbModel(DbSchema_Mysql)]
    public class User_Test : TimestampGuidDbModel
    {
        public string Name { get; set; } = null!;
    }

    [DbModel(DbSchema_Mysql)]
    public class UserProfile_Test : TimestampGuidDbModel
    {
        [DbForeignKey(typeof(User_Test), true)]
        public Guid UserId { get; set; }

        public int Age { get; set; }
    }
}