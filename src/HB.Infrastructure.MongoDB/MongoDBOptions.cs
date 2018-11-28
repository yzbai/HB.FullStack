using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HB.Infrastructure.MongoDB
{
    public class MongoDBConnectionSetting
    {
        public string InstanceName { get; set; }
        public string ConnectionString { get; set; }
    }

    public class MongoDBOptions : IOptions<MongoDBOptions>
    {
        public IList<MongoDBConnectionSetting> ConnectionSettings { get; set; } = new List<MongoDBConnectionSetting>();

        public MongoDBOptions Value => this;

        public string GetConnectionString(string instanceName)
        {
            if (string.IsNullOrEmpty(instanceName))
            {
                throw new ArgumentNullException(nameof(instanceName));
            }

            return ConnectionSettings.First(s => instanceName.Equals(s.InstanceName, GlobalSettings.Comparison))?.ConnectionString;
        }
    }
}
