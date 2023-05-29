using HB.FullStack.Common;
using HB.FullStack.Database.DbModels;

using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Server.Identity.Models
{
    //public interface IUserRole
    //{
    //    object RoleId { get; set; }
    //    object UserId { get; set; }
    //}

    /// <summary>
    /// 用户-角色 关系 实体
    /// </summary>
    //TODO: 把关系表定义到各个相关实体中，而不单独设置关系Model
    public class UserRole<TId> : DbModel<TId>, ITimestamp
    {
        [NoEmptyGuid]
        [DbForeignKey(typeof(User<>), false)]
        public TId UserId { get; set; } = default!;

        [NoEmptyGuid]
        [DbForeignKey(typeof(Role<>), false)]
        public TId RoleId { get; set; } = default!;

        public UserRole() { }

        public UserRole(TId userId, TId roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }

        public override TId Id { get; set; } = default!;
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }
}