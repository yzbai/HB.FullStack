/*
 * Author：Yuzhao Bai
 * Email: yzbai@brlite.com
 * Github: github.com/yzbai
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using HB.FullStack.Common;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Models
{
    public class User_Test : DbModel2<Guid>, ITimestamp
    {
        public string Name { get; set; } = null!;
        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class UserProfile_Test : DbModel2<Guid>, ITimestamp
    {
        [DbForeignKey(typeof(User_Test), true)]
        public Guid UserId { get; set; }

        public int Age { get; set; }
        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }
}