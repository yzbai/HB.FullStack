using System.ComponentModel.DataAnnotations;
using HB.FullStack.Common.Entities;
using HB.FullStack.Common.IdGen;

namespace HB.FullStack.Database.Def
{
    public abstract class IdEntity : DatabaseEntity2
    {
        [PrimaryKey]
        [EntityProperty(0)]
        [CacheKey]
        [LongId]
        public long Id { get; set; } = IDistributedIdGen.IdGen.GetId();
    }
}
