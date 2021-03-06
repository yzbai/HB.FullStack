﻿using HB.FullStack;
using HB.FullStack.Benchmark.Database;
using HB.FullStack.Database;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OrmBenchmark.Core;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;

namespace OrmBenchmark.ConsoleUI.NetCore
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            // Set up configuration sources.
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
               .AddEnvironmentVariables()
               .SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile("appsettings.json", optional: false)
               .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: false);

            IConfigurationRoot configuration = configurationBuilder.Build();

            string connStr = configuration["MySQL:Connections:0:ConnectionString"];

            // Set up data directory
            string runDir = System.AppContext.BaseDirectory;
            //string runDir = AppDomain.CurrentDomain.BaseDirectory;
            connStr = connStr.Replace("|DataDirectory|", runDir.TrimEnd('\\'));

            bool warmUp = false;

            var benchmarker = new Benchmarker(connStr, 500);



            //benchmarker.RegisterOrmExecuter(new Ado.PureAdoExecuter());

            benchmarker.RegisterOrmExecuter(new Dapper.DapperExecuter());
            benchmarker.RegisterOrmExecuter(new Dapper.DapperContribExecuter());
            //benchmarker.RegisterOrmExecuter(new EntityFramework.EntityFrameworkExecuter());
            //benchmarker.RegisterOrmExecuter(new EntityFramework.EntityFrameworNoTrackingExecuter());
            benchmarker.RegisterOrmExecuter(new OrmLite.OrmLiteNoQueryExecuter());
            benchmarker.RegisterOrmExecuter(new FullStackDatabaseExecutor());



            //benchmarker.RegisterOrmExecuter(new Ado.PureAdoExecuterGetValues());
            //benchmarker.RegisterOrmExecuter(new SimpleData.SimpleDataExecuter());

            //benchmarker.RegisterOrmExecuter(new Dapper.DapperBufferedExecuter());
            //benchmarker.RegisterOrmExecuter(new Dapper.DapperFirstOrDefaultExecuter());

            //benchmarker.RegisterOrmExecuter(new PetaPoco.PetaPocoExecuter());
            //benchmarker.RegisterOrmExecuter(new PetaPoco.PetaPocoFastExecuter());
            //benchmarker.RegisterOrmExecuter(new PetaPoco.PetaPocoFetchExecuter());
            //benchmarker.RegisterOrmExecuter(new PetaPoco.PetaPocoFetchFastExecuter());
            //benchmarker.RegisterOrmExecuter(new OrmToolkit.OrmToolkitExecuter());
            //benchmarker.RegisterOrmExecuter(new OrmToolkit.OrmToolkitNoQueryExecuter());
            ////benchmarker.RegisterOrmExecuter(new OrmToolkit.OrmToolkitAutoMapperExecuter());
            //benchmarker.RegisterOrmExecuter(new OrmToolkit.OrmToolkitTestExecuter());


            //benchmarker.RegisterOrmExecuter(new InsightDatabase.InsightDatabaseExecuter());
            //benchmarker.RegisterOrmExecuter(new InsightDatabase.InsightSingleDatabaseExecuter());
            //benchmarker.RegisterOrmExecuter(new OrmLite.OrmLiteExecuter());


            //benchmarker.RegisterOrmExecuter(new DevExpress.DevExpressQueryExecuter());


            Console.WriteLine("ORM Benchmark");

            Console.Write("\nDo you like to have a warm-up stage(y/[n])?");
            var str = Console.ReadLine();
            if (str.Trim().ToLower() == "y" || str.Trim().ToLower() == "yes")
                warmUp = true;

            var ver = Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
            Console.WriteLine(ver);
            Console.WriteLine("Connection string: {0}", connStr);
            Console.Write("\nRunning...");

            await PrepareDatabaseAsync().ConfigureAwait(false);


            await benchmarker.RunAsync(warmUp).ConfigureAwait(false);
            Console.WriteLine("Finished.");

            Console.ForegroundColor = ConsoleColor.Red;

            if (warmUp)
            {
                Console.WriteLine("\nPerformance of Warm-up:");
                ShowResults(benchmarker.resultsWarmUp, false, false);
            }

            Console.WriteLine("\nPerformance of select and map a row to a POCO object over 500 iterations:");
            ShowResults(benchmarker.results, true);

            Console.WriteLine("\nPerformance of select and map a row to a Dynamic object over 500 iterations:");
            ShowResults(benchmarker.resultsForDynamicItem, true);

            Console.WriteLine("\nPerformance of mapping 5000 rows to POCO objects in one iteration:");
            ShowResults(benchmarker.resultsForAllItems);

            Console.WriteLine("\nPerformance of mapping 5000 rows to Dynamic objects in one iteration:");
            ShowResults(benchmarker.resultsForAllDynamicItems);

            Console.ReadLine();
        }

        /// <summary>
        /// PrepareDatabaseAsync
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DatabaseException">Ignore.</exception>
        private static async System.Threading.Tasks.Task PrepareDatabaseAsync()
        {
            ServiceFixture serviceFixture = new ServiceFixture();

            IDatabase database = serviceFixture.ServiceProvider.GetRequiredService<IDatabase>();

            await database.InitializeAsync().ConfigureAwait(false);

            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < 2000; ++i)
            {
                stringBuilder.Append('x');
            }

            string text = stringBuilder.ToString();

            List<Post> posts = new List<Post>();

            for (int i = 0; i < 5001; i++)
            {
                Post newItem = new Post { Text = text, CreationDate = TimeUtil.UtcNowUnixTimeMilliseconds, LastChangeDate = TimeUtil.UtcNowUnixTimeMilliseconds };
                posts.Add(newItem);
            }

            ITransaction transaction = serviceFixture.ServiceProvider.GetRequiredService<ITransaction>();
            TransactionContext transactionContext = await transaction.BeginTransactionAsync<Post>().ConfigureAwait(false);
            try
            {
                await database.BatchAddAsync(posts, "", transactionContext).ConfigureAwait(false);

                await transaction.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await transaction.RollbackAsync(transactionContext).ConfigureAwait(false);
            }

        }

        static void ShowResults(List<BenchmarkResult> results, bool showFirstRun = false, bool ignoreZeroTimes = true)
        {
            var defaultColor = Console.ForegroundColor;
            //Console.ForegroundColor = ConsoleColor.Gray;

            int i = 0;
            var list = results.OrderBy(o => o.ExecTime);
            if (ignoreZeroTimes)
                list = results.FindAll(o => o.ExecTime > 0).OrderBy(o => o.ExecTime);

            foreach (var result in list)
            {
                Console.ForegroundColor = i < 3 ? ConsoleColor.Green : ConsoleColor.Gray;

                if (showFirstRun)
                    Console.WriteLine(string.Format("{0,2}-{1,-40} {2,5} ms (First run: {3,3} ms)", ++i, result.Name, result.ExecTime, result.FirstItemExecTime));
                else
                    Console.WriteLine(string.Format("{0,2}-{1,-40} {2,5} ms", ++i, result.Name, result.ExecTime));
            }

            Console.ForegroundColor = defaultColor;
        }
    }
}
