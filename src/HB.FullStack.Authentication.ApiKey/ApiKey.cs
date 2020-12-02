using HB.Framework.Database.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Authentication.ApiKey
{
    public class ApiKey : DatabaseEntity
    {
        [UniqueGuidEntityProperty]
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();


    }
}
