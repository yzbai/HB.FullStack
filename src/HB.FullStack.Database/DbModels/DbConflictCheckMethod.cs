using System;

namespace HB.FullStack.Database.DbModels
{
    [Flags]
    public enum DbConflictCheckMethods
    {
        Ignore = 0,
        OldNewValueCompare = 1,
        Timestamp = 2 
    }
}