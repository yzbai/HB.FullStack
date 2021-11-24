using System.Collections.Generic;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Base
{
    public abstract class NavigationManager
    {
        private static NavigationManager? _current;

        public static NavigationManager Current
        {
            get
            {
                if (_current == null)
                {
                    _current = DependencyService.Resolve<NavigationManager>();
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
        public abstract Task GotoAsync(string uri, bool animated = false);

        public abstract Task GotoAsync(string uri, IDictionary<string, string> parameters, bool animated = false);

        public abstract Task GotoAsync(Page page, bool animated = false);

        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="animated"></param>
        /// <returns></returns>
        public abstract Task GoBackAsync(bool animated = false);

        #region 传统的，不需要再Shell的Routings中登记的，需要成对使用

        public abstract Task PushAsync(Page page, bool animated = false);

        public abstract Task PopAsync(bool animated = false);

        public abstract Task PushModalAsync(Page page, bool animated = false);

        public abstract Task PopModalAsync(bool animated = false);

        #endregion
    }
}