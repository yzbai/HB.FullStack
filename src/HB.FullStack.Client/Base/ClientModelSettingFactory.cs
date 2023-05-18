/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Reflection;

using HB.FullStack.Common;

using Microsoft.Extensions.Options;

namespace HB.FullStack.Client.Base
{
    public class ClientModelSettingFactory : IClientModelSettingFactory
    {
        private readonly Dictionary<Type, ClientModelSetting> _clientModelDefs = new Dictionary<Type, ClientModelSetting>();

        private readonly ClientOptions _clientOptions;

        public ClientModelSettingFactory(IOptions<ClientOptions> options)
        {
            _clientOptions = options.Value;
        }

        public ClientModelSetting? Get<T>() where T : IModel
        {
            Type type = typeof(T);

            if (_clientModelDefs.TryGetValue(type, out ClientModelSetting? def))
            {
                return def;
            }

            //TODO: Use Source Generation
            SyncSettingAttribute? localDataAttribute = type.GetCustomAttribute<SyncSettingAttribute>(true);

            if (localDataAttribute == null)
            {
                return null;
            }

            ClientModelSetting newDef = new ClientModelSetting
            {
                //ExpiryTime = localDataAttribute.ExpiryTimeType switch
                //{
                //    ExpiryTimeType.Always => TimeSpan.FromSeconds(0),
                //    ExpiryTimeType.Tiny => TimeSpan.FromSeconds(_clientOptions.TinyExpirySeconds),
                //    ExpiryTimeType.Short => TimeSpan.FromSeconds(_clientOptions.ShortExpirySeconds),
                //    ExpiryTimeType.Medium => TimeSpan.FromSeconds(_clientOptions.MediumExpirySeconds),
                //    ExpiryTimeType.Long => TimeSpan.FromSeconds(_clientOptions.LongExpirySeconds),
                //    ExpiryTimeType.NonExpiry => TimeSpan.FromSeconds(int.MaxValue),
                //    _ => TimeSpan.FromSeconds(0),
                //},

                AllowOfflineRead = localDataAttribute.AllowOfflineRead,
                AllowOfflineAdd = localDataAttribute.AllowOfflineAdd,
                AllowOfflineDelete = localDataAttribute.AllowOfflineDelete,
                AllowOfflineUpdate = localDataAttribute.AllowOfflineUpdate
            };

            if (_clientModelDefs.TryAdd(type, newDef))
            {
                return newDef;
            }

            return _clientModelDefs[type];
        }

        public void Register<T>(ClientModelSetting setting) where T : IModel
        {
            _clientModelDefs[typeof(T)] = setting;
        }
    }
}