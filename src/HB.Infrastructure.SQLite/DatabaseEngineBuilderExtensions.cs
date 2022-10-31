/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using HB.FullStack.Database.Engine;
using HB.Infrastructure.SQLite;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DatabaseEngineBuilderExtensions
    {
        public static IDatabaseEngineBuilder AddSQLite(this IDatabaseEngineBuilder builder)
        {
            SQLitePCL.Batteries_V2.Init();

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDatabaseEngine, SQLiteEngine>());

            return builder;
        }
    }
}