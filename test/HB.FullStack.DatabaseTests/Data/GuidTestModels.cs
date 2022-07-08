using HB.FullStack.Database.DatabaseModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Tests.Mocker
{
    public class User : GuidDatabaseModel
    {
        public string Name { get; set; } = null!;


    }

    public class UserProfile : GuidDatabaseModel
    {
        [ForeignKey(typeof(User), true)]
        public Guid UserId { get; set; }

        public int Age { get; set; }
    }
}
