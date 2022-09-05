using HB.FullStack.Database.DbModels;

using System;

namespace HB.FullStack.Identity.Models
{
    public class UserActivity : TimestampGuidDbModel
    {
        [ForeignKey(typeof(User), false)]
        public Guid? UserId { get; set; }

        [ForeignKey(typeof(SignInToken), false)]
        public Guid? SignInTokenId { get; set; }

        public string? Ip { get; set; }

        [DBModelProperty(MaxLength = LengthConventions.MAX_URL_LENGTH)]
        public string? Url { get; set; }

        [DBModelProperty(MaxLength = 10)]
        public string? HttpMethod { get; set; }

        [DBModelProperty(MaxLength = LengthConventions.MAX_ARGUMENTS_LENGTH)]
        public string? Arguments { get; set; }

        public int? ResultStatusCode { get; set; }

        public string? ResultType { get; set; }

        [DBModelProperty(MaxLength = LengthConventions.MAX_RESULT_ERROR_LENGTH)]
        public string? ResultError { get; set; }
    }
}