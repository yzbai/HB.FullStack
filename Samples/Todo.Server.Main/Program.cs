using HB.FullStack.Server.WebLib.Startup;

using Todo.Shared;

internal class Program
{
    private static void Main(string[] args)
    {
        SharedSettings sharedSettings = SharedSettings.GetSharedSettings(EnvironmentUtil.AspNetCoreEnvironment.ThrowIfNullOrEmpty("AspNetCoreEnvironment is Empty."));

        WebApiStartup.Run<long>(args, new WebApiStartupSettings(
            services => { },
            initHostOptions => { },
            directoryOptions =>
            {
                directoryOptions.AliyunOssEndpoint = sharedSettings.AliyunOssEndpoint;
                directoryOptions.AliyunOssBucketName = sharedSettings.AliyunOssBucketName;
                directoryOptions.DirectoryDescriptions = sharedSettings.DirectoryDescriptions;
                directoryOptions.DirectoryPermissions = sharedSettings.DirectoryPermissions;
            }));
    }
}

