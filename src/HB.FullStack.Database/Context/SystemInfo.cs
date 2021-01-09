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
                if (_sysDict.TryGetValue(SystemInfoNames.DatabaseName, out string? value))
                {
                    return value;
                }

                throw new DatabaseException(DatabaseErrorCode.DatabaseNameNotFoundInSystemInfoTable);
            }
            private set
            {
                _sysDict[SystemInfoNames.DatabaseName] = value;
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
                if (_sysDict.TryGetValue(SystemInfoNames.Version, out string? value))
                {
                    return Convert.ToInt32(value, CultureInfo.InvariantCulture);
                }

                throw new DatabaseException(DatabaseErrorCode.VersionNotFoundInSystemInfoTable);
            }
            set
            {
                _sysDict[SystemInfoNames.Version] = value.ToString(GlobalSettings.Culture);
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