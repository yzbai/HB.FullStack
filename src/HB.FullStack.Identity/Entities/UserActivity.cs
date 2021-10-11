using HB.FullStack.Database.Entities;

using System;

namespace HB.FullStack.Identity.Entities
{
    public class UserActivity : GuidEntity
    {
        [ForeignKey(typeof(User), false)]
        public Guid? UserId { get; set; }

        [ForeignKey(typeof(SignInToken), false)]
        public Guid? SignInTokenId { get; set; }

        public string? Ip { get; set; }

        [EntityProperty(MaxLength = LengthConventions.MAX_URL_LENGTH)]
        public string? Url { get; set; }

        [EntityProperty(MaxLength = 10)]
        public string? HttpMethod { get; set; }

        [EntityProperty(MaxLength = LengthConventions.MAX_ARGUMENTS_LENGTH)]
        public string? Arguments { get; set; }

        public int? ResultStatusCode { get; set; }

        public string? ResultType { get; set; }

        [EntityProperty(MaxLength = LengthConventions.MAX_RESULT_ERROR_LENGTH)]
        public string? ResultError { get; set; }
    }
}