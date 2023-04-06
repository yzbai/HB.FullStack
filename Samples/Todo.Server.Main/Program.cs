using HB.FullStack.Web;
using HB.FullStack.Web.Startup;

using Microsoft.AspNetCore.HttpOverrides;

using Serilog;

internal class Program
{
    private static void Main(string[] args)
    {
        StartupSettings startupSettings = new StartupSettings(services => { }, initHostOptions => { });
        HBFullStackStartup.RunWebApi(args, startupSettings);
    }
}

