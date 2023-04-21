/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System.Collections.Generic;

using HB.FullStack.Client.MauiLib.Base;

namespace HB.FullStack.Client.MauiLib.Controls
{
    /// <summary>
    /// 通过 Navigation 返回 IsSucceed
    /// </summary>
    public partial class CropperPage : BasePage<CropperViewModel>
    {
        public const string Query_CroppedSucceed = "CroppedSucceed";

        public CropperPage()
        {
            InitializeComponent();
        }

        protected override void RegisterCustomerControls(IList<IBaseContentView> customerControls)
        {
            customerControls.Add(FigureCanvas);
        }
    }
}