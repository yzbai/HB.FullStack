/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using HB.Infrastructure.SQLite;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DatabaseEngineBuilderExtensions
    {
        public static IDbEngineBuilder AddSQLite(this IDbEngineBuilder builder)
        {
            SQLitePCL.Batteries_V2.Init();

            builder.AddDatabaseEngine<SQLiteEngine>();  

            return builder;
        }
    }
}