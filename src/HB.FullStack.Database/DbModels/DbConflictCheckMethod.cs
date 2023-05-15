using System;

namespace HB.FullStack.Database.DbModels
{
    [Flags]
    public enum DbConflictCheckMethods
    {
        Ignore = 1,
        OldNewValueCompare = 2,
        Timestamp = 4 
    }
}