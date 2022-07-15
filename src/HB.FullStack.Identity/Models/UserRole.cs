﻿using HB.FullStack.Database.DBModels;

using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Identity.Models
{
    /// <summary>
    /// 用户-角色 关系 实体
    /// </summary>
    //TODO: 把关系表定义到各个相关实体中，而不单独设置关系Model
    public class UserRole : TimelessFlackIdDBModel
    {
        [NoEmptyGuid]
        [ForeignKey(typeof(User), false)]
        public Guid UserId { get; set; }

        [NoEmptyGuid]
        [ForeignKey(typeof(Role), false)]
        public Guid RoleId { get; set; }

        public UserRole() { }

        public UserRole(Guid userId, Guid roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }
    }
}