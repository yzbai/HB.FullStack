using HB.Framework.Common.Entity;
using System;


namespace HB.Framework.Database.Entity
{
    /// <summary>
    /// 数据库表类型。 一个TableDomain对应一张数据库表。
    /// 所有数据库表类型必须继承此类。
    /// </summary>
    //[Serializable]
    public class DatabaseEntity : CommonEntity
    {
        [DatabaseMainKey("Id")]
        public long Id { get; set; } = -1;

        [DatabaseEntityProperty("Version")]
        public long Version { get; set; } = 0;

        [DatabaseEntityProperty("上一次更改者", Length = 10)]
        public String LastUser { get; set; } = string.Empty;

        [DatabaseEntityProperty("上一次更改时间")]
        public DateTimeOffset? LastTime { get; set; }

        [DatabaseEntityProperty("逻辑删除标志")]
        public Boolean Deleted { get; set; } = false;
    }
}

