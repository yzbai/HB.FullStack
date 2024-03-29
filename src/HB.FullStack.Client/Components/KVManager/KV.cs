﻿using System;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client.Components.KVManager
{
    public class KV : TimelessGuidDbModel
    {
        public string Key { get; set; } = null!;

        public string? Value { get; set; }

        public DateTimeOffset ExpiredAt { get; set; }
    }
}