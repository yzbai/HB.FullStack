using HB.FullStack.Client.Maui.Base;

using Microsoft.Maui.Controls;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace HB.FullStack.Client.Maui.Controls.Cropper
{
    public partial class CropperPage : BaseContentPage
    {
        public CropperPage(string imageFullPath, string croppedImageFullPath, Action<bool> onSucceed) 
            : base(new CropperViewModel(imageFullPath, croppedImageFullPath, onSucceed))
        {
            InitializeComponent();
        }
    }
}