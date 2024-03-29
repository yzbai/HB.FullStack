﻿using HB.FullStack.Common.Shared;
using HB.FullStack.Database.DbModels;

using System;

namespace HB.FullStack.Server.Identity.Models
{
    //TODO: 改用KVStore
    public class UserActivity : TimestampGuidDbModel
    {
        [DbForeignKey(typeof(User), false)]
        public Guid? UserId { get; set; }

        [DbForeignKey(typeof(TokenCredential), false)]
        public Guid? SignInCredentialId { get; set; }

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
    }
}