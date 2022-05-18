using System.Collections.Generic;

namespace HB.FullStack.XamarinForms.Base
{
    public interface IBaseContentView
    {
        /// <summary>
        /// 是否已展示
        /// </summary>
        bool IsAppearred { get; }

        /// <summary>
        /// 初始化
        /// </summary>
        void OnAppearing();


        /// <summary>
        /// 收拾处理，准备结束
        /// </summary>
        void OnDisappearing();

        IList<IBaseContentView?>? GetAllCustomerControls();
    }
}
