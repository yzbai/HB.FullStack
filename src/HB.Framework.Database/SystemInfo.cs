using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Database
{
    public static class SystemInfoNames
    {
        public static string Version = "Version";
        public static string DatabaseName = "DatabaseName";
    }
    public class SystemInfo
    {
        public string DatabaseName => _sysDict[SystemInfoNames.DatabaseName];

        public int Version => Convert.ToInt32(_sysDict[SystemInfoNames.Version]);

        private IDictionary<string, string> _sysDict = new Dictionary<string, string>();

        public SystemInfo()
        {

        }
    }
}
