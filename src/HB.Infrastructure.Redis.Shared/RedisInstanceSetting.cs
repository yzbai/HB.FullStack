using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

using StackExchange.Redis;

namespace HB.Infrastructure.Redis.Shared
{
    public class RedisInstanceSetting
    {
        private int? _databaseNumber;
        private EndPoint? _serverEndPoint;

        [DisallowNull, NotNull]
        public string InstanceName { get; set; } = null!;

        [DisallowNull, NotNull]
        public ConnectionString ConnectionString { get; set; } = null!;

        public int DatabaseNumber
        {
            get
            {
                if (!_databaseNumber.HasValue)
                {
                    ParseConfiguration();
                }
                return _databaseNumber!.Value;
            }
            set
            {
                _databaseNumber = value;
            }
        }

        internal EndPoint ServerEndPoint
        {
            get
            {
                if (_serverEndPoint == null)
                {
                    ParseConfiguration();
                }

                return _serverEndPoint!;
            }
        }

        private void ParseConfiguration()
        {
            ConfigurationOptions configurationOptions = ConfigurationOptions.Parse(ConnectionString.ToString());

            if (configurationOptions.DefaultDatabase.HasValue)
            {
                _databaseNumber = configurationOptions.DefaultDatabase;
            }
            else
            {
                _databaseNumber = 0;
            }

            _serverEndPoint = configurationOptions.EndPoints[0];
        }
    }
}