using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Database
{
    public class UpdatePackTimestamp
    {
        [Required]
        public object? Id { get; set; }

        [Required]
        public long? OldTimestamp { get; set; }

        /// <summary>
        /// 如果不指定，则使用TimeUtil.Timestamp
        /// </summary>
        public long? NewTimestamp { get; set; }

        public IList<string> PropertyNames { get; set; } = new List<string>();

        public IList<object?> NewPropertyValues { get; set; } = new List<object?>();
        
    }

    public static class UpdatePackTimestampExtensions
    {
        public static UpdatePackTimestamp ThrowIfNotValid(this UpdatePackTimestamp updatePack)
        {
            if (!updatePack.OldTimestamp.HasValue || updatePack.OldTimestamp.Value <= 638000651894004864)
            {
                throw DbExceptions.TimestampShouldBePositive(updatePack.OldTimestamp ?? 0);
            }

            if (updatePack.NewTimestamp.HasValue && updatePack.NewTimestamp.Value <= 638000651894004864)
            {
                throw DbExceptions.TimestampShouldBePositive(updatePack.NewTimestamp.Value);
            }

            if (updatePack.Id is long longId && longId <= 0)
            {
                throw DbExceptions.LongIdShouldBePositive(longId);
            }

            if (updatePack.Id is Guid guid && guid.IsEmpty())
            {
                throw DbExceptions.GuidShouldNotEmpty();
            }

            if (updatePack.PropertyNames.Count != updatePack.NewPropertyValues.Count)
            {
                throw DbExceptions.UpdateUsingTimestampListCountNotEqual();
            }

            if (updatePack.PropertyNames.Count <= 0)
            {
                throw DbExceptions.UpdateUsingTimestampListEmpty();
            }

            return updatePack;
        }

    }
}
