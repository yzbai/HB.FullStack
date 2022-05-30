using HB.FullStack.Client.Maui.Base;

using System;
using System.Diagnostics.CodeAnalysis;

namespace HB.FullStack.Client.Maui.Controls.Cropper
{
    
    public partial class CropperPage : BaseContentPage
    {
        public CropperPage(string imageFullPath, string croppedImageFullPath, Action<bool> onCroppFinish) 
            : base(new CropperViewModel(imageFullPath, croppedImageFullPath, onCroppFinish))
        {
            InitializeComponent();
        }        
    }
}