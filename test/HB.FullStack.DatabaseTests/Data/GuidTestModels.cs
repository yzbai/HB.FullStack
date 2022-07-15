using System;

using HB.FullStack.Database.DBModels;

namespace HB.FullStack.Tests.Mocker
{
    public class User : TimestampGuidDBModel
    {
        public string Name { get; set; } = null!;


    }

    public class UserProfile : TimestampGuidDBModel
    {
        [ForeignKey(typeof(User), true)]
        public Guid UserId { get; set; }

        public int Age { get; set; }
    }
}
