﻿using System;
using System.Collections.Generic;
using System.Text;
namespace HB.FullStack.Repository
{
    static class Consts
    {
        //TODO: put this into options
        public static readonly TimeSpan OccupiedTime = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan PatienceTime = TimeSpan.FromSeconds(2);
    }
}
