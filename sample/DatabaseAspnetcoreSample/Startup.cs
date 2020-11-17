using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HB.Framework.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseAspnetcoreSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options => {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            //ø¥’‚¿Ô
            //you can not use both
            //services.AddMySQL(Configuration.GetSection("MySQL"));
            services.AddSQLite(Configuration.GetSection("SQLite"));



            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IDatabase database)
        {
            database.Initialize(GetMigrations());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc();
        }

        public IList<Migration> GetMigrations()
        {
            //return new List<Migration> {
            //    new Migration{
            //     OldVersion = 1,
            //     NewVersion = 2,
            //     TargetSchema="test_db",
            //     SqlStatement ="alter columns, move data, add more tables"
            //    },
            //     new Migration{
            //     OldVersion = 2,
            //     NewVersion = 99,
            //     TargetSchema="test_db",
            //     SqlStatement ="alter columns, move data, add more tables"
            //    },
            //    new Migration{
            //     OldVersion = 99,
            //     NewVersion =100,
            //     TargetSchema="test_db",
            //     SqlStatement ="alter columns, move data, add more tables"
            //    },
            //    new Migration{
            //     OldVersion = 1,
            //     NewVersion =2,
            //     TargetSchema="test_another_db",
            //     SqlStatement ="alter columns, move data, add more tables"
            //    }
            //};

            return null;
        }
    }
}
