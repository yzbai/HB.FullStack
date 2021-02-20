using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.XamarinForms;
using HB.FullStack.XamarinForms.Base;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace HB.FullStack.XamarinForms
{
    public abstract class NavigationService
    {
        public static NavigationService Current { get; set; } = null!;

        public INavigation? Navigation { get; protected set; }

        public void PopToRoot(bool animated = true)
        {
            Device.BeginInvokeOnMainThread(() => Navigation?.PopToRootAsync(animated).Fire());
        }

        public void Pop(bool animated = true)
        {
            Device.BeginInvokeOnMainThread(() => Navigation?.PopAsync(animated).Fire());
        }

        public void Push(Page page, bool animated = true)
        {
            Device.BeginInvokeOnMainThread(() => Navigation?.PushAsync(page, animated).Fire());
        }

        public void PushModal(Page page, bool animate = true)
        {
            Device.BeginInvokeOnMainThread(() => Navigation?.PushModalAsync(page, animate).Fire());
        }

        public void PopModal(bool animated = true)
        {
            Device.BeginInvokeOnMainThread(() => Navigation?.PopModalAsync(animated).Fire());
        }

        public abstract void PopoutPage<TPage>(bool animated = true) where TPage : Page;

        public abstract void PopOrPopoutPageIfAfter<TPage>(bool animated = true) where TPage : Page;

        public abstract void PushLoginPage(bool animated = true);

    }
}
