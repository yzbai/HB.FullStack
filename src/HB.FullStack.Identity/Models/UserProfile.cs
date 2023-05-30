using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Server.Identity.Models
{
    //public interface IUserProfile
    //{
    //    string? AvatarFileName { get; set; }
    //    DateOnly? BirthDay { get; set; }
    //    Gender? Gender { get; set; }
    //    string NickName { get; set; }
    //    object UserId { get; set; }
    //}

    public class UserProfile<TId> : DbModel<TId>
    {
        [DbForeignKey(typeof(User<>), true)]
        public TId UserId { get; set; } = default!;

        [NickName(CanBeNull = false)]
        public string NickName { get; set; } = null!;

        public Gender? Gender { get; set; }

        public DateOnly? BirthDay { get; set; }

        [DbField(MaxLength = SharedNames.Length.MAX_FILE_NAME_LENGTH)]
        public string? AvatarFileName { get; set; }
        public override TId Id { get; set; } = default!;
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
    }
}
