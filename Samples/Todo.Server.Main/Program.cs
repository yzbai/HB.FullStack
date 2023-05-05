using HB.FullStack.Server.WebLib.Startup;

using Todo.Shared;

internal class Program
{
    private static void Main(string[] args)
    {
        SharedSettings sharedSettings = SharedSettings.GetSharedSettings(EnvironmentUtil.AspNetCoreEnvironment.ThrowIfNullOrEmpty("AspNetCoreEnvironment is Empty."));

        WebApiStartup.Run(args, new WebApiStartupSettings(
            services => { },
            initHostOptions => { },
            directoryOptions =>
            {
                directoryOptions.DirectoryDescriptions = sharedSettings.DirectoryDescriptions;
                directoryOptions.DirectoryPermissions = sharedSettings.DirectoryPermissions;
            }));
    }
}

