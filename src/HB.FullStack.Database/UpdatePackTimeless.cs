using System;
using System.Collections.Generic;

namespace HB.FullStack.Database
{
    /// <summary>
    /// 使用新旧值比较来解决冲突，不使用Timestamp来解决冲突
    /// </summary>
    public class UpdatePackTimeless
    {
        public object? Id { get; set; }

        public IList<string> PropertyNames { get; set; } = new List<string>();

        public IList<object?> NewPropertyValues { get; set; } = new List<object?>();

        public IList<object?> OldPropertyValues { get; set; } = new List<object?>();
    }

    public static class UpdatePackTimelessExtensions
    {
        public static UpdatePackTimeless ThrowIfNotValid(this UpdatePackTimeless updatePack)
        {
            if (updatePack.Id is long longId && longId <= 0)
            {
                throw DbExceptions.LongIdShouldBePositive(longId);
            }

            if (updatePack.Id is Guid guid && guid.IsEmpty())
            {
                throw DbExceptions.GuidShouldNotEmpty();
            }

            if (updatePack.PropertyNames.Count != updatePack.NewPropertyValues.Count || updatePack.OldPropertyValues.Count != updatePack.PropertyNames.Count)
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
