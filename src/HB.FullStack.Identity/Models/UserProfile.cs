using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared.Context;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Server.Identity.Models
{
    public class UserProfile : TimestampGuidDbModel
    {
        [NoEmptyGuid]
        [DbForeignKey(typeof(User), true)]
        public Guid UserId { get; set; }

        public string? Level { get; set; }

        [NickName(CanBeNull = false)]
        public string NickName { get; set; } = null!;

        public Gender? Gender { get; set; }

        public DateOnly? BirthDay { get; set; }

        [DbField(MaxLength = LengthConventions.MAX_FILE_NAME_LENGTH)]
        public string? AvatarFileName { get; set; }
    }
}
