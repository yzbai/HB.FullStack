using HB.FullStack.XamarinForms.TCaptcha;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace HB.FullStack.XamarinForms.Base
{
    public interface INavigationService
    {
        private static INavigationService? _current;
        public static INavigationService Current
        {
            get
            {
                if(_current == null)
                {
                    _current = DependencyService.Resolve<INavigationService>();
                }

                return _current;
            }
        }

        //void PushLoginPage(bool animated);
        //void PopModal();
        //void Pop();
        //void PushModal(TCaptchaDialog dialog, bool v);

        /// <summary>
        /// 用于登记在
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task GotoAsync(string uri, bool animated = false);

        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="animated"></param>
        /// <returns></returns>
        Task GoBackAsync(bool animated = false);



        #region 传统的，不需要再Shell的Routings中登记的，需要成对使用

        Task PushAsync(ContentPage page, bool animated = false);

        Task PopAsync(ContentPage page, bool animated = false);

        Task PushModalAsync(ContentPage page, bool animated = false);

        Task PopModalAsync(ContentPage page, bool animated = false);

        #endregion
    }
}
