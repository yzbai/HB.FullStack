using System.Collections.Generic;

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