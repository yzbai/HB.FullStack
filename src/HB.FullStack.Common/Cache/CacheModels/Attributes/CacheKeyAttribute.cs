﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CacheKeyAttribute : Attribute
    {

    }
}
