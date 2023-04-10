using HB.FullStack.Common.ApiClient;

using Microsoft.Extensions.DependencyInjection;

namespace Todo.Client.ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ServiceCollection services = new ServiceCollection();





            IServiceProvider serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<IConsoleInitializeService>().Initialize();

            serviceProvider.GetRequiredService<TaskExecutor>().RunDown();

            Console.ReadLine();
        }
    }

    internal class TaskExecutor
    {
        private readonly IApiClient _apiClient;

        public TaskExecutor(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        internal void RunDown()
        {
            //_apiClient.SendAsync()
        }
    }

    internal interface IConsoleInitializeService
    {
        void Initialize();
    }

    internal class ConsoleInitializeService : IConsoleInitializeService
    {
        public void Initialize()
        {
            
        }
    }
}