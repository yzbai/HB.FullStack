using Microsoft.Maui.Controls;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.FullStack.Client.MauiLib.Utils
{
    public static class NavigationExtensions
    {
        public static async Task GoBackAsync(this Shell shell, IDictionary<string, object?>? parameters = null)
        {
            if (parameters == null)
            {
                await shell.GoToAsync("..");
            }
            else
            {
                await shell.GoToAsync("..", parameters);
            }
        }

        public static Task GoBackAsync(this INavigation navigation, IDictionary<string, object?>? parameters = null) => Shell.Current.GoBackAsync(parameters);


        public static async Task GoToAsync(this INavigation navigation, ShellNavigationState state, IDictionary<string, object?>? parameters = null)
        {
            if (parameters == null)
            {
                await Shell.Current.GoToAsync(state);
            }
            else
            {
                await Shell.Current.GoToAsync(state, parameters);
            }
        }
    }

}