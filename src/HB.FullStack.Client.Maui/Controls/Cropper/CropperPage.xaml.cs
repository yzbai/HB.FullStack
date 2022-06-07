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
    public partial class CropperPage : BasePage
    {
        public const string Query_CroppedSucceed = "CroppedSucceed";

        public CropperPage(string imageFullPath, string croppedImageFullPath) 
            : base(new CropperViewModel(imageFullPath, croppedImageFullPath))
        {
            InitializeComponent();
        }
    }
}