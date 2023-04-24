using HB.FullStack.Client.Abstractions;
using HB.FullStack.Client.ApiClient;

namespace Todo.Client.ConsoleApp
{
    internal class TaskExecutor
    {
        private readonly IApiClient _apiClient;
        private readonly ITokenPreferences _preferenceProvider;

        public TaskExecutor(IApiClient apiClient, ITokenPreferences preferenceProvider)
        {
            _apiClient = apiClient;
            _preferenceProvider = preferenceProvider;
        }

        internal async Task RunDownAsync()
        {
            try
            {
                string loginName = "yuzhaobai" + DateTimeOffset.UtcNow.Ticks;
                string password = "Password123";    
                
                //Register
                await _apiClient.RegisterByLoginNameAsync(loginName, password);

                //Login
                await _apiClient.LoginByLoginNameAsync(loginName, password);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}