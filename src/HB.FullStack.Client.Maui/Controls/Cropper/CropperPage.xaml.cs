using HB.FullStack.Client.Maui.Base;

using Microsoft.Maui.Controls;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HB.FullStack.Client.Maui.Controls.Cropper
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