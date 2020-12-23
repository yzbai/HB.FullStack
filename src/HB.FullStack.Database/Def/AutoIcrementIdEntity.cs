using System.ComponentModel.DataAnnotations;
using HB.FullStack.Common.Entities;

namespace HB.FullStack.Database.Def
{
    public abstract class AutoIcrementIdEntity : DatabaseEntity2
    {
        [AutoIncrementPrimaryKey2]
        [EntityProperty(0)]
        [CacheKey]
        public long Id { get; set; } = -1;
    }
}
