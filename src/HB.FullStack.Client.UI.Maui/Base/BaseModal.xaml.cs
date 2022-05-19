using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Toolkit.Mvvm.Input;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.FullStack.Client.UI.Maui.Base;

public partial class BaseModal : BaseContentPage
{
    public bool IsBackgroudClickedToDismiss { get; set; }

    public BaseModal(BaseViewModel? viewModel):base(viewModel)
	{
		InitializeComponent();

        Shell.SetPresentationMode(this, PresentationMode.ModalAnimated);

        BackgroundColor = Color.FromArgb("#80000000");
    }
    protected override IList<IBaseContentView> GetAllCustomerControls()=>new List<IBaseContentView>();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        ControlTemplate = (ControlTemplate)Resources["BaseModalTemplate"];
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        StackLayout modalContainer = (StackLayout)GetTemplateChild("BaseModalContainer");

        TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
        tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;

        modalContainer.GestureRecognizers.Add(tapGestureRecognizer);
    }

    private async void TapGestureRecognizer_Tapped(object? sender, System.EventArgs e)
    {
        if (!IsBackgroudClickedToDismiss)
        {
            return;
        }

        await INavigationManager.Current!.GoBackAsync().ConfigureAwait(false);
    }
}