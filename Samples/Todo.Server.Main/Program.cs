using HB.FullStack.Server.WebLib.Startup;

internal class Program
{
    private static void Main(string[] args)
    {
        WebApiStartup.Run(args, new WebApiStartupSettings(services => { }, initHostOptions => { }));
    }
}

