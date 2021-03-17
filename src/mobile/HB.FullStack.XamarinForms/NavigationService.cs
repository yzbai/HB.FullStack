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
        private readonly Func<INavigation> _navigationFunc;

        public static NavigationService Current { get; set; } = null!;

        public static void Init(NavigationService navigationService)
        {
            Current = navigationService;
        }

        protected NavigationService(Func<INavigation> navigationFunc)
        {
            _navigationFunc = navigationFunc;
        }

        public INavigation Navigation { get => _navigationFunc(); }

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

        /// <summary>
        /// 当前navStack[0]的PageType
        /// </summary>
        public Type? CurrentRootPageType { get; set; }

        public void PopoutPage<TPage>(bool animated = true)
        {
            var navStack = Navigation?.NavigationStack;

            if (navStack == null || navStack.Count == 0)
            {
                return;
            }

            //Shell的坑navStack[0]永远是null, 所以不能用navStack[0] is TPage来判断
            if (CurrentRootPageType == typeof(TPage))
            {
                ResumeRouting();
            }
            else
            {
                List<Page> toRemoves = new List<Page>();

                for (int i = navStack.Count - 1; i >= 0; --i)
                {
                    if (navStack[i] is not TPage)
                    {
                        toRemoves.Add(navStack[i]);
                    }
                    else
                    {
                        break;
                    }
                }

                Device.BeginInvokeOnMainThread(async () =>
                {
                    toRemoves.ForEach(p => Navigation?.RemovePage(p));

                    await Navigation!.PopAsync(animated).ConfigureAwait(true);
                });
            }
        }

        /// <summary>
        /// 如果当前页在TPage后，则一直弹出TPage；否则，只是Pop
        /// </summary>
        /// <typeparam name="TPage"></typeparam>
        /// <param name="animated"></param>
        public void PopOrPopoutPageIfAfter<TPage>(bool animated = true)
        {
            if (Navigation == null)
            {
                return;
            }

            if (CurrentRootPageType == typeof(TPage))
            {
                ResumeRouting();
            }

            foreach (var page in Navigation.NavigationStack)
            {
                if (page is TPage)
                {
                    PopoutPage<TPage>(animated);
                    return;
                }
            }

            Pop();
        }

        /// <summary>
        /// 返回当前NavStac栈顶的PageType
        /// </summary>
        /// <returns></returns>
        public abstract void ResumeRouting();

        public abstract void PushLoginPage(bool animated = true);

    }
}
