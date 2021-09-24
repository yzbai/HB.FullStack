using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database;
using HB.FullStack.Database.Entities;
using HB.FullStack.Identity.Entities;

namespace HB.FullStack.Identity.Entities
{
    public class UserActivityEntity : GuidEntity
    {
        public const int MAX_URL_LENGTH = 2000;
        public const int MAX_ARGUMENTS_LENGTH = 2000;
        public const int MAX_RESULT_ERROR_LENGTH = 2000;


        [ForeignKey(typeof(UserEntity))]
        public Guid? UserId { get; set; }

        [ForeignKey(typeof(SignInTokenEntity))]
        public Guid? SignInTokenId { get; set; }
        
        public string? Ip { get; set; }

        [EntityProperty(MaxLength = MAX_URL_LENGTH)]
        public string? Url { get; set; }

        [EntityProperty(MaxLength = 10)]
        public string? HttpMethod { get; set; }

        [EntityProperty(MaxLength = MAX_ARGUMENTS_LENGTH)]
        public string? Arguments { get; set; }

        public int? ResultStatusCode { get; set; }
        
        public string? ResultType { get; set; }
        
        [EntityProperty(MaxLength = MAX_RESULT_ERROR_LENGTH)]
        public string? ResultError { get; set; }
    }
}
