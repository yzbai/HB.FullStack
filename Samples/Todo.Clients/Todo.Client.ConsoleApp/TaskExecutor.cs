using HB.FullStack.Client.Abstractions;
using HB.FullStack.Client.ApiClient;
using HB.FullStack.Client.Components.Users;

namespace Todo.Client.ConsoleApp
{
    internal class TaskExecutor
    {
        private readonly IApiClient _apiClient;
        private readonly ITokenPreferences _preferenceProvider;
        private readonly IUserService _userService;

        public TaskExecutor(IApiClient apiClient, ITokenPreferences preferenceProvider, IUserService userService)
        {
            _apiClient = apiClient;
            _preferenceProvider = preferenceProvider;
            _userService = userService;
        }

        internal async Task RunDownAsync()
        {
            try
            {
                string loginName = "yuzhaobai" + DateTimeOffset.UtcNow.Ticks;
                string password = "Password123";    
                
                //Register
                await _userService.RegisterByLoginNameAsync(loginName, password);

                //Login
                await _userService.LoginByLoginNameAsync(loginName, password);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}