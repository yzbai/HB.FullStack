using CommunityToolkit.Maui.Views;

namespace HB.FullStack.Client.Maui.Controls.Popups
{
    public partial class ExceptionPopup : Popup
    {
        public string Message { get; set; }

        public ExceptionPopup(PopupSizeConstants popupSizeConstants, string message,  bool needConfirm)
        {
            InitializeComponent();

            Message = message;
            Size = needConfirm? popupSizeConstants.Medium : popupSizeConstants.Tiny;

            BindingContext = this;
        }

        private void Button_Clicked(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}