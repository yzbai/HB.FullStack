using System;

namespace HB.FullStack.Database.DbModels
{
    [Flags]
    public enum ConflictCheckMethods
    {
        Ignore = 0,
        OldNewValueCompare = 1,
        Timestamp = 2
    }
}