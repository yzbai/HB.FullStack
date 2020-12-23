using System;
using HB.FullStack.Common.Entities;

namespace HB.FullStack.Database.Def
{
    public abstract class AutoIncrementIdGuidEntity : AutoIncrementIdEntity
    {

        [UniqueGuidEntityProperty(1)]
        [CacheKey]
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();
    }

    public abstract class IdGenGuidEntity : IdGenEntity
    {

        [UniqueGuidEntityProperty(1)]
        [CacheKey]
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();
    }
}
