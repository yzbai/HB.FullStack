using HB.Component.Identity.Entity;
using HB.Framework.Common;
using System.Collections.Generic;

namespace HB.Component.Identity.Test
{
    public class DataMocker
    {
        public static IList<User> MockUsers()
        {
            string prefix = SecurityHelper.CreateRandomString(6);

            IList<User> list = new List<User>();

            for (int i = 0; i < 1000; ++i)
            {
                User user = new User() {
                    Guid = SecurityHelper.CreateUniqueToken(),
                    SecurityStamp = SecurityHelper.CreateUniqueToken(),
                    UserName = $"{prefix}_{SecurityHelper.CreateUniqueToken()}",
                    Mobile = $"1{SecurityHelper.CreateRandomNumbericString(10)}",
                    IsActivated = true
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
