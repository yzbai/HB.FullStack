using HB.FullStack.KVStore.Engine;

using Microsoft.Extensions.DependencyInjection;

namespace HB.FullStack.KVStore.Config
{
    public interface IKVStoreEngineBuilder
    {
        IServiceCollection Services { get; }

        void AddKVStoreEngine<TDbEngine>() where TDbEngine : class, IKVStoreEngine;
    }
}
