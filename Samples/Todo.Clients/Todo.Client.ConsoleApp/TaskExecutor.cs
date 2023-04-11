﻿using HB.FullStack.Common.ApiClient;

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
                await _apiClient.RegisterByLoginNameAsync("yuzhaobai" + DateTimeOffset.Now.Ticks, Program.SITE_TODO_SERVER_MAIN, "Password123");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}