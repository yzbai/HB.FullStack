using System.ComponentModel.DataAnnotations;
using HB.FullStack.Common.Entities;

namespace HB.FullStack.Database.Def
{
    public abstract class AutoIncrementIdEntity : IdDatabaseEntity
    {
        [AutoIncrementPrimaryKey]
        [EntityProperty(0)]
        [CacheKey]
        public override long Id { get; set; } = -1;
    }
}
