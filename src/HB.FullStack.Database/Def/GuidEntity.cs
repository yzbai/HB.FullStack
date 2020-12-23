using System;
using HB.FullStack.Common.Entities;

namespace HB.FullStack.Database.Def
{
    public abstract class GuidEntity : DatabaseEntity
    {
        [PrimaryKey]
        [UniqueGuidEntityProperty(0)]
        [CacheKey]
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();
    }
}
