using HB.Component.Identity.Entity;
using HB.Framework.Common;
using System;
using System.Collections.Generic;

namespace HB.Component.Identity.Test
{
    public class DataMocker
    {
        public static IList<User> MockUsers()
        {
            string prefix = SecurityUtil.CreateRandomString(6) + "测试用户";

            IList<User> list = new List<User>();

            for (int i = 0; i < 1000; ++i)
            {
                User user = new User() {
                    Guid = SecurityUtil.CreateUniqueToken(),
                    SecurityStamp = SecurityUtil.CreateUniqueToken(),
                    UserName = $"{prefix}_{SecurityUtil.CreateUniqueToken()}",
                    Mobile = $"1{SecurityUtil.CreateRandomNumbericString(10)}",
                    IsActivated = true,
                    UserType = "TestUsers"
                    
                };

                list.Add(user);
            }

            return list;
        }

        public static IList<Role> MockRoles()
        {
            IList<Role> list = new List<Role>
            {
                new Role() { Name = "Admin", DisplayName = "Diplay-Admin", IsActivated = true, Comment = "xxxxx" },
                new Role() { Name = "Level1", DisplayName = "Diplay-Level1", IsActivated = true, Comment = "xxxxx" },
                new Role() { Name = "Level2", DisplayName = "Diplay-Level2", IsActivated = true, Comment = "xxxxx" },
                new Role() { Name = "Level3", DisplayName = "Diplay-Level3", IsActivated = true, Comment = "xxxxx" }
            };

            return list;
        }
    }
}
