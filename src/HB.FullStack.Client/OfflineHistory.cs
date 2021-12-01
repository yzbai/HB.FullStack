﻿using HB.FullStack.Database.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client
{
    public enum DbOperation
    {
        Add,
        Update,
        Delete,
    }

    /// <summary>
    /// 使用自增保证顺序
    /// </summary>
    public class OfflineHistory : AutoIncrementIdEntity
    {
        public string EntityId { get; set; } = null!;

        public string EntityFullName { get; set; } = null!;

        public DbOperation Operation { get; set; }

        public DateTimeOffset OperationTime { get; set; }

        public bool Handled { get; set; }

    }
}