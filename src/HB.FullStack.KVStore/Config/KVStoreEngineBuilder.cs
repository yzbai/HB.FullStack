using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.KVStore.Engine;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HB.FullStack.KVStore.Config
{
    internal sealed class KVStoreEngineBuilder : IKVStoreEngineBuilder
    {
        public IServiceCollection Services { get; }

        public KVStoreEngineBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public void AddKVStoreEngine<TDbEngine>() where TDbEngine : class, IKVStoreEngine
        {
            Services.AddSingleton<IKVStoreEngine, TDbEngine>();
        }
    }
}
