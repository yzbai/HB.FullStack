using HB.FullStack.Database;
using HB.FullStack.Database.Engine;
using HB.Infrastructure.MySQL;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DatabaseEngineBuilderExtensions
    {
        public static IDatabaseEngineBuilder AddMySQL(this IDatabaseEngineBuilder builder)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDatabaseEngine, MySQLEngine>());

            return builder;
        }
    }
}