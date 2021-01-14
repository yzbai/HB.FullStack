using HB.FullStack.Mobile.Base;

using System;
using System.Collections.Generic;
using System.Windows.Input;

using Xamarin.CommunityToolkit.Converters;
using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;

namespace HB.FullStack.Mobile.Controls
{
    public class Chip : BaseContentView
    {
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(Chip), null, BindingMode.OneWay);

        public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(Chip));

        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(double), typeof(Chip), Device.GetNamedSize(NamedSize.Default, typeof(Label)));

        public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(Chip), Color.DarkGray);

        public static readonly BindableProperty ImageProperty = BindableProperty.Create(nameof(ImageSource), typeof(ImageSource), typeof(Chip));

        public static readonly BindableProperty ClickedCommandProperty = BindableProperty.Create(nameof(ClickedCommand), typeof(ICommand), typeof(Chip));

        public static readonly BindableProperty ClickedCommandParameterProperty = BindableProperty.Create(nameof(ClickedCommandParameter), typeof(object), typeof(Chip));

        public static readonly BindableProperty CloseCommandProperty = BindableProperty.Create(nameof(CloseCommand), typeof(ICommand), typeof(Chip));

        public static readonly BindableProperty CloseCommandParameterProperty = BindableProperty.Create(nameof(CloseCommandParameter), typeof(object), typeof(Chip));

        public static readonly BindableProperty CloseImageProperty = BindableProperty.Create(nameof(CloseImageSource), typeof(ImageSource), typeof(Chip));

        public static readonly BindableProperty IsToggleableProperty = BindableProperty.Create(nameof(IsToggleable), typeof(bool), typeof(Chip), propertyChanged: OnIsSelectedPropertyChanged);

        public static readonly BindableProperty AutoToggleProperty = BindableProperty.Create(nameof(AutoToggle), typeof(bool), typeof(Chip));

        public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(Chip), false, BindingMode.TwoWay, propertyChanged: OnIsSelectedPropertyChanged);

        public static readonly BindableProperty SelectCommandProperty = BindableProperty.Create(nameof(SelectCommand), typeof(ICommand), typeof(Chip));

        public static readonly BindableProperty SelectCommandParameterProperty = BindableProperty.Create(nameof(SelectCommandParameterProperty), typeof(object), typeof(Chip));

        public static readonly BindableProperty UnselectCommandProperty = BindableProperty.Create(nameof(UnselectCommand), typeof(ICommand), typeof(Chip));

        public static readonly BindableProperty UnselectCommandParameterProperty = BindableProperty.Create(nameof(UnselectCommandParameterProperty), typeof(object), typeof(Chip));

        public static readonly BindableProperty UnselectedBackgroundColorProperty = BindableProperty.Create(nameof(UnselectedBackgroundColor), typeof(Color), typeof(Chip), Color.White);

        public static readonly BindableProperty SelectedBackgroundColorProperty = BindableProperty.Create(nameof(SelectedBackgroundColor), typeof(Color), typeof(Chip), Color.Gray);

        public static readonly BindableProperty UnselectedHasShadowProperty = BindableProperty.Create(nameof(UnselectedHasShadow), typeof(bool), typeof(Chip), true);

        public static readonly BindableProperty SelectedHasShadowProperty = BindableProperty.Create(nameof(SelectedHasShadow), typeof(bool), typeof(Chip), true);

        private readonly WeakEventManager _eventManager = new WeakEventManager();

        public event EventHandler OnClicked
        {
            add => _eventManager.AddEventHandler(value);
            remove => _eventManager.RemoveEventHandler(value);
        }

        public event EventHandler OnClose
        {
            add => _eventManager.AddEventHandler(value);
            remove => _eventManager.RemoveEventHandler(value);
        }

        public event EventHandler OnSelect
        {
            add => _eventManager.AddEventHandler(value);
            remove => _eventManager.RemoveEventHandler(value);
        }

        public event EventHandler OnUnselect
        {
            add => _eventManager.AddEventHandler(value);
            remove => _eventManager.RemoveEventHandler(value);
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string FontFamily
        {
            get => (string)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        [Xamarin.Forms.TypeConverter(typeof(FontSizeConverter))]
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        public ICommand ClickedCommand
        {
            get => (ICommand)GetValue(ClickedCommandProperty);
            set => SetValue(ClickedCommandProperty, value);
        }

        public object ClickedCommandParameter
        {
            get => GetValue(ClickedCommandParameterProperty);
            set => SetValue(ClickedCommandParameterProperty, value);
        }

        public ICommand CloseCommand
        {
            get => (ICommand)GetValue(CloseCommandProperty);
            set => SetValue(CloseCommandProperty, value);
        }

        public object CloseCommandParameter
        {
            get => GetValue(CloseCommandParameterProperty);
            set => SetValue(CloseCommandParameterProperty, value);
        }

        public ImageSource CloseImageSource
        {
            get => (ImageSource)GetValue(CloseImageProperty);
            set => SetValue(CloseImageProperty, value);
        }

        public bool IsToggleable
        {
            get => (bool)GetValue(IsToggleableProperty);
            set => SetValue(IsToggleableProperty, value);
        }

        public bool AutoToggle
        {
            get => (bool)GetValue(AutoToggleProperty);
            set => SetValue(AutoToggleProperty, value);
        }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public ICommand SelectCommand
        {
            get => (ICommand)GetValue(SelectCommandProperty);
            set => SetValue(SelectCommandProperty, value);
        }

        public object SelectCommandParameter
        {
            get => GetValue(SelectCommandParameterProperty);
            set => SetValue(SelectCommandParameterProperty, value);
        }

        public ICommand UnselectCommand
        {
            get => (ICommand)GetValue(UnselectCommandProperty);
            set => SetValue(UnselectCommandProperty, value);
        }

        public object UnselectCommandParameter
        {
            get => GetValue(UnselectCommandParameterProperty);
            set => SetValue(UnselectCommandParameterProperty, value);
        }

        public Color UnselectedBackgroundColor
        {
            get => (Color)GetValue(UnselectedBackgroundColorProperty);
            set => SetValue(UnselectedBackgroundColorProperty, value);
        }

        public Color SelectedBackgroundColor
        {
            get => (Color)GetValue(SelectedBackgroundColorProperty);
            set => SetValue(SelectedBackgroundColorProperty, value);
        }

        public bool UnselectedHasShadow
        {
            get => (bool)GetValue(UnselectedHasShadowProperty);
            set => SetValue(UnselectedHasShadowProperty, value);
        }

        public bool SelectedHasShadow
        {
            get => (bool)GetValue(SelectedHasShadowProperty);
            set => SetValue(SelectedHasShadowProperty, value);
        }

        public string? GroupName { get; set; }

        public object? Tag { get; set; }


        private static void OnIsSelectedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != oldValue)
                ((Chip)bindable).RefreshVisualState();
        }

        private Frame _frame;
        public Chip()
        {
            Content = new Frame
            {
                CornerRadius = 15,
                HasShadow = false,
                Content = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Children = {
                        new Image{ }
                            .Bind(Image.IsVisibleProperty, nameof(ImageSource), convert: (object? obj)=>obj!=null),

                        new Label{ }
                            .Center()
                            .TextCenter()
                            .Bind(Label.TextProperty, nameof(Text))
                            .Bind(Label.TextColorProperty, nameof(TextColor))
                            .Bind(Label.IsVisibleProperty, nameof(Text), convert:(object? obj)=>obj!=null)
                            .Bind(Label.FontFamilyProperty, nameof(FontFamily))
                            .Bind(Label.FontSizeProperty, nameof(FontSize)),

                        new Image{ }
                            .Bind(Image.SourceProperty, nameof(CloseImageSource))
                            .Bind(Image.IsVisibleProperty, nameof(CloseImageSource), convert : (object? obj)=>obj!=null)
                            .Invoke(v=>v.GestureRecognizers.Add(new TapGestureRecognizer{ }.Invoke(v=>v.Tapped += CloseButton_Clicked)))
                    }
                }.Center()
            }.Center().Margin(0, 0).Padding(8, 5)
            .Invoke(v => v.GestureRecognizers.Add(new TapGestureRecognizer { }.Invoke(v => v.Tapped += Clicked)))
            .Invoke(v => v.BindingContext = this)
            .Assign(out _frame);

            VisualState selectedState = new VisualState { Name = "Selected" };
            selectedState.Setters.AddBinding(Frame.BackgroundColorProperty, new Binding(nameof(SelectedBackgroundColor), source: this));
            selectedState.Setters.AddBinding(Frame.HasShadowProperty, new Binding(nameof(SelectedHasShadow), source: this));

            VisualState unSelectedState = new VisualState { Name = "UnSelected" };
            unSelectedState.Setters.AddBinding(Frame.BackgroundColorProperty, new Binding(nameof(UnselectedBackgroundColor), source: this));
            unSelectedState.Setters.AddBinding(Frame.HasShadowProperty, new Binding(nameof(UnselectedHasShadow), source: this));

            VisualStateGroup commonStateGroup = new VisualStateGroup { Name = "CommonStates" };
            commonStateGroup.States.Add(selectedState);
            commonStateGroup.States.Add(unSelectedState);

            VisualStateManager.SetVisualStateGroups(_frame, new VisualStateGroupList { commonStateGroup });

            SizeChanged += (object? sender, EventArgs e) =>
            {
                _frame.CornerRadius = (float)(Height * 0.5);
            };
        }

        public override void OnAppearing()
        {
            base.OnAppearing();
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        private void RefreshVisualState()
        {
            string stateName = IsToggleable ? (IsSelected ? "Selected" : "Unselected") : "Normal";
            VisualStateManager.GoToState(_frame, stateName);
        }

        private void Clicked(object sender, EventArgs args)
        {
            _eventManager.HandleEvent(sender, args, nameof(OnClicked));
            //OnClicked?.Invoke(sender, args);

            if (ClickedCommand != null && ClickedCommand.CanExecute(ClickedCommandParameter))
                ClickedCommand.Execute(ClickedCommandParameter);

            if (IsToggleable)
            {
                if (!string.IsNullOrEmpty(GroupName) && Parent is Layout<View> layout)
                {
                    foreach (View view in layout.Children)
                    {
                        if (view is Chip chip)
                        {
                            if (GroupName.Equals(chip.GroupName, GlobalSettings.Comparison))
                            {
                                chip.IsSelected = false;
                                RefreshVisualState();
                            }
                        }
                    }

                }

                if (AutoToggle)
                {
                    IsSelected = !IsSelected;
                    RefreshVisualState();
                }

                if (IsSelected)
                {
                    _eventManager.HandleEvent(this, new EventArgs(), nameof(OnSelect));
                    //OnSelect?.Invoke(this, new EventArgs());

                    if (SelectCommand != null && SelectCommand.CanExecute(SelectCommandParameter))
                        SelectCommand.Execute(SelectCommandParameter);
                }
                else
                {
                    _eventManager.HandleEvent(this, new EventArgs(), nameof(OnUnselect));
                    //OnUnselect?.Invoke(this, new EventArgs());

                    if (UnselectCommand != null && SelectCommand.CanExecute(UnselectCommandParameter))
                        UnselectCommand.Execute(UnselectCommandParameter);
                }
            }
        }

        private void CloseButton_Clicked(object sender, EventArgs args)
        {
            _eventManager.HandleEvent(sender, args, nameof(OnClose));
            //OnClose?.Invoke(sender, args);

            if (CloseCommand != null && CloseCommand.CanExecute(CloseCommandParameter))
                CloseCommand.Execute(CloseCommandParameter);
        }

        public override IList<IBaseContentView?>? GetAllCustomerControls()
        {
            return null;
        }
    }
}
