using HB.FullStack.Common.ApiClient;

namespace Todo.Client.ConsoleApp
{
    internal class TaskExecutor
    {
        private readonly IApiClient _apiClient;
        private readonly IPreferenceProvider _preferenceProvider;

        public TaskExecutor(IApiClient apiClient, IPreferenceProvider preferenceProvider)
        {
            _apiClient = apiClient;
            _preferenceProvider = preferenceProvider;
        }

        internal async Task RunDownAsync()
        {
            try
            {
                string loginName = "yuzhaobai" + DateTimeOffset.UtcNow.Ticks;
                string audience = Program.SITE_TODO_SERVER_MAIN;
                string password = "Password123";    
                
                //Register
                await _apiClient.RegisterByLoginNameAsync(loginName, password, audience);

                //Login
                await _apiClient.LoginByLoginNameAsync(loginName, password, audience);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}