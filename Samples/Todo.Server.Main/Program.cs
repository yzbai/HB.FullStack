using HB.FullStack.Server.WebLib.Startup;

using Todo.Shared;

internal class Program
{
    private static void Main(string[] args)
    {
        WebApiStartup.Run(args, new WebApiStartupSettings(
            services => { },
            initHostOptions => { },
            directoryOptions =>
            {
                directoryOptions.DirectoryDescriptions = DirectorySettings.Descriptions.All;
                directoryOptions.DirectoryPermissions = DirectorySettings.Permissions.All;
            }));
    }
}

