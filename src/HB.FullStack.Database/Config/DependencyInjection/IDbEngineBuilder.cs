using HB.FullStack.Database.Engine;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IDbEngineBuilder
    {
        IServiceCollection Services { get; }

        IDbEngineBuilder AddDatabaseEngine<TImplement>() where TImplement : class, IDbEngine;
    }
}