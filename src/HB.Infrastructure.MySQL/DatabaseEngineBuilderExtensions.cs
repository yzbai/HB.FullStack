using HB.Infrastructure.MySQL;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DatabaseEngineBuilderExtensions
    {
        public static IDbEngineBuilder AddMySQL(this IDbEngineBuilder builder)
        {

            builder.AddDatabaseEngine<MySQLEngine>();

            return builder;
        }
    }
}