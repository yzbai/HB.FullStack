using HB.FullStack.Server.Startup;

internal class Program
{
    private static void Main(string[] args)
    {
        WebApiStartup.Run(args, new WebApiStartupSettings(services => { }, initHostOptions => { }));
    }
}

