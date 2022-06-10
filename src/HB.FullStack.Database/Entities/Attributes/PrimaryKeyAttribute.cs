﻿using System;

namespace HB.FullStack.Database.Entities
{
    [AttributeUsage(AttributeTargets.Property)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1813:Avoid unsealed attributes", Justification = "<Pending>")]
    public class PrimaryKeyAttribute : Attribute
    {
    }
}
