﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.IdGen;

namespace HB.FullStack.Database.Entities
{
    public abstract class DatabaseEntity : Entity
    {
    }

    public abstract class IdDatabaseEntity : DatabaseEntity
    {
        public abstract long Id { get; set; }
    }

    public abstract class AutoIncrementIdEntity : IdDatabaseEntity
    {
        [AutoIncrementPrimaryKey]
        [EntityProperty(0)]
        [CacheKey]
        public override long Id { get; set; } = -1;
    }

    public abstract class IdGenEntity : IdDatabaseEntity
    {
        [PrimaryKey]
        [EntityProperty(0)]
        [CacheKey]
        [LongId]
        public override long Id
        {
            get; set;
        } = StaticIdGen.GetId();
    }

    //public abstract class AutoIncrementIdGuidEntity : AutoIncrementIdEntity
    //{
    //    [Required]
    //    [UniqueGuidEntityProperty(1)]
    //    [CacheKey]
    //    public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();
    //}

    //public abstract class IdGenGuidEntity : IdGenEntity
    //{
    //    [Required]
    //    [UniqueGuidEntityProperty(1)]
    //    [CacheKey]
    //    public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();
    //}

    public abstract class GuidEntity : DatabaseEntity
    {
        [Required]
        [PrimaryKey]
        [UniqueGuidEntityProperty(0)]
        [CacheKey]
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();
    }
}
