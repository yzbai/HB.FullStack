using HB.FullStack.Database.DatabaseModels;

using System;

namespace HB.FullStack.Identity.Models
{
    public class UserActivity : GuidDatabaseModel
    {
        [ForeignKey(typeof(User), false)]
        public Guid? UserId { get; set; }

        [ForeignKey(typeof(SignInToken), false)]
        public Guid? SignInTokenId { get; set; }

        public string? Ip { get; set; }

        [DatabaseModelProperty(MaxLength = LengthConventions.MAX_URL_LENGTH)]
        public string? Url { get; set; }

        [DatabaseModelProperty(MaxLength = 10)]
        public string? HttpMethod { get; set; }

        [DatabaseModelProperty(MaxLength = LengthConventions.MAX_ARGUMENTS_LENGTH)]
        public string? Arguments { get; set; }

        public int? ResultStatusCode { get; set; }

        public string? ResultType { get; set; }

        [DatabaseModelProperty(MaxLength = LengthConventions.MAX_RESULT_ERROR_LENGTH)]
        public string? ResultError { get; set; }
    }
}