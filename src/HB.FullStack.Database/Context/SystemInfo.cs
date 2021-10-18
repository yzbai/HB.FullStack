#nullable enable

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
        /// DatabaseName
        /// </summary>
        /// <exception cref="DatabaseException">Get.</exception>
        [NotNull, DisallowNull]
        public string DatabaseName
        {
            get
            {
                if (_sysDict.TryGetValue(SystemInfoNames.DATABASE_NAME, out string? value))
                {
                    return value;
                }

                throw Exceptions.SystemInfoError(cause:"no DatabaseName key in SystemInfoTable");
            }
            private set
            {
                _sysDict[SystemInfoNames.DATABASE_NAME] = value;
            }
        }

        /// <summary>
        /// Version
        /// </summary>
        /// <exception cref="DatabaseException">Get.</exception>
        public int Version
        {
            get
            {
                if (_sysDict.TryGetValue(SystemInfoNames.VERSION, out string? value))
                {
                    return Convert.ToInt32(value, GlobalSettings.Culture);
                }

                throw Exceptions.SystemInfoError(cause:"no Version key in SystemInfoTable");
            }
            set
            {
                _sysDict[SystemInfoNames.VERSION] = value.ToString(GlobalSettings.Culture);
            }
        }

        public SystemInfo(string databaseName)
        {
            DatabaseName = databaseName;
        }

        public void Set(string name, string value)
        {
            _sysDict[name] = value;
        }
    }
}