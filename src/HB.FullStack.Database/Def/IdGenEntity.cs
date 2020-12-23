using System.ComponentModel.DataAnnotations;
using HB.FullStack.Common.Entities;
using HB.FullStack.Common.IdGen;

namespace HB.FullStack.Database.Def
{

    public abstract class IdGenEntity : IdDatabaseEntity
    {
        [PrimaryKey]
        [EntityProperty(0)]
        [CacheKey]
        [LongId]
        public override long Id { get; set; } = IDistributedIdGen.IdGen.GetId();
    }
}
