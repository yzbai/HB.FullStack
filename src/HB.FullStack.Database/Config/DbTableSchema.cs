﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HB.FullStack.Database.Config
{
    
    public class DbTableSchema
    {
        [DisallowNull, NotNull]
        public string TableName { get; set; } = null!;

        [DisallowNull, NotNull]
        public string DbModelFullName { get; set; } = null!;

        public bool? ReadOnly { get; set; }

        public IList<DbFieldSchema> Fields { get; set; } = new List<DbFieldSchema>();
    }
}