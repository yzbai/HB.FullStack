

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace HB.FullStack.Database
{
    /// <summary>
    /// 内部表tb_sys_info中的键值对
    /// </summary>
    internal class SystemInfo
    {
        private readonly IDictionary<string, string> _sysDict = new Dictionary<string, string>();

        /// <summary>
        /// DatabaseSchema
        /// </summary>

        [NotNull, DisallowNull]
        public string DatabaseSchema
        {
            get
            {
                if (_sysDict.TryGetValue(SystemInfoNames.DATABASE_SCHEMA, out string? value))
                {
                    return value;
                }

                throw DatabaseExceptions.SystemInfoError(cause: "no DatabaseSchema key in SystemInfoTable");
            }
            private set
            {
                _sysDict[SystemInfoNames.DATABASE_SCHEMA] = value;
            }
        }

        /// <summary>
        /// Version
        /// </summary>

        public int Version
        {
            get
            {
                if (_sysDict.TryGetValue(SystemInfoNames.VERSION, out string? value))
                {
                    return System.Convert.ToInt32(value, Globals.Culture);
                }

                throw DatabaseExceptions.SystemInfoError(cause: "no Version key in SystemInfoTable");
            }
            set
            {
                _sysDict[SystemInfoNames.VERSION] = value.ToString(Globals.Culture);
            }
        }

        public SystemInfo(string dbSchema)
        {
            DatabaseSchema = dbSchema;
        }

        public void Set(string name, string value)
        {
            _sysDict[name] = value;
        }
    }
}