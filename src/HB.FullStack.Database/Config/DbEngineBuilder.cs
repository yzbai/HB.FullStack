using HB.FullStack.Database.Engine;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    internal sealed class DbEngineBuilder : IDbEngineBuilder
    {
        public IServiceCollection Services { get; }

        public DbEngineBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IDbEngineBuilder AddDatabaseEngine<TDbEngine>() where TDbEngine : class, IDbEngine
        {
            Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDbEngine, TDbEngine>());

            return this;
        }
    }
}