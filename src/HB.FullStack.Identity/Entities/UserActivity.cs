using HB.FullStack.Database.Entities;

using MessagePack;

using System;

namespace HB.FullStack.Identity.Entities
{
    [MessagePackObject]
    public class UserActivity : GuidEntity
    {
        [ForeignKey(typeof(User), false)]
        [Key(7)]
        public Guid? UserId { get; set; }

        [ForeignKey(typeof(SignInToken), false)]
        [Key(8)]
        public Guid? SignInTokenId { get; set; }

        [Key(9)]
        public string? Ip { get; set; }

        [EntityProperty(MaxLength = LengthConventions.MAX_URL_LENGTH)]
        [Key(10)]
        public string? Url { get; set; }

        [EntityProperty(MaxLength = 10)]
        [Key(11)]
        public string? HttpMethod { get; set; }

        [EntityProperty(MaxLength = LengthConventions.MAX_ARGUMENTS_LENGTH)]
        [Key(12)]
        public string? Arguments { get; set; }

        [Key(13)]
        public int? ResultStatusCode { get; set; }

        [Key(14)]
        public string? ResultType { get; set; }

        [EntityProperty(MaxLength = LengthConventions.MAX_RESULT_ERROR_LENGTH)]
        [Key(15)]
        public string? ResultError { get; set; }
    }
}