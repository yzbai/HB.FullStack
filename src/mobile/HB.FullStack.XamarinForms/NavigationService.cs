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

        public void PopToRoot()
        {
            Device.BeginInvokeOnMainThread(() => Navigation?.PopToRootAsync().Fire());
        }

        public void Pop()
        {
            Device.BeginInvokeOnMainThread(() => Navigation?.PopAsync().Fire());
        }

        public void Push(Page page, bool animated = true)
        {
            Device.BeginInvokeOnMainThread(() => Navigation?.PushAsync(page, animated).Fire());
        }

        public void PushModal(Page page, bool animate = true)
        {
            Device.BeginInvokeOnMainThread(() => Navigation?.PushModalAsync(page, animate).Fire());
        }

        public abstract void PopoutRegisterProfilePage();

        public void PopModal()
        {
            Device.BeginInvokeOnMainThread(()=>Navigation?.PopModalAsync().Fire());
        }

        public abstract void ResetMainPage();

        public abstract void PushLoginPage(bool animated = true);
        
        public abstract void PopoutLogin(bool animated = true);

        public abstract void PopoutIntroduce();

        
    }
}
