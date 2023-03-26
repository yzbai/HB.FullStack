using HB.FullStack.WebApi;

internal class Program
{
    private static void Main(string[] args)
    {
        try
        {
            SerilogHelper.OpenLogs();

            EnvironmentUtil.EnsureEnvironment();

            Globals.Logger.LogStarup();

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            ConfigureBuilder(builder);



            WebApplication app = builder.Build();
            GlobalWebApplicationAccessor.Application = app;

            ConfigureApplication(app);

            app.Run();
        }
        catch (Exception ex)
        {
            Globals.Logger.LogCriticalShutDown(ex);
        }
        finally
        {
            SerilogHelper.CloseLogs();
        }
    }


    private static void ConfigureBuilder(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
    }

    private static void ConfigureApplication(WebApplication app)
    {
        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

    }
}