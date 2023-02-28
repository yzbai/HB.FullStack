using System;

using HB.FullStack.Common.PropertyTrackable;

namespace HB.FullStack.Database.DbModels
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DbPrimaryKeyAttribute : AddtionalPropertyAttribute
    {
    }
}
