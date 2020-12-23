using System;
using HB.FullStack.Common.Entities;

namespace HB.FullStack.Database.Def
{
    public abstract class IdGuidEntity : AutoIcrementIdEntity
    {

        [UniqueGuidEntityProperty(1)]
        [CacheKey]
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();
    }
}
