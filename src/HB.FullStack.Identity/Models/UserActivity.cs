using HB.FullStack.Common;
using HB.FullStack.Common.Shared;
using HB.FullStack.Database.DbModels;

using System;

namespace HB.FullStack.Server.Identity.Models
{
    public class UserActivity<TId> : DbModel<TId>, ITimestamp
    {
        [DbForeignKey(typeof(User<>), false)]
        public TId? UserId { get; set; }

        [DbForeignKey(typeof(TokenCredential<>), false)]
        public TId? SignInCredentialId { get; set; }

        public string? Ip { get; set; }

        [DbField(MaxLength = SharedNames.Length.MAX_URL_LENGTH)]
        public string? Url { get; set; }

        [DbField(MaxLength = 10)]
        public string? HttpMethod { get; set; }

        [DbField(MaxLength = SharedNames.Length.MAX_ARGUMENTS_LENGTH)]
        public string? Arguments { get; set; }

        public int? ResultStatusCode { get; set; }

        public string? ResultType { get; set; }

        [DbField(MaxLength = SharedNames.Length.MAX_RESULT_ERROR_LENGTH)]
        public string? ResultError { get; set; }
        public override TId Id { get; set; } = default!;
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }
}