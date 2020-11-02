using System;
using System.Collections.Generic;
using HB.Framework.Client.Base;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HB.Framework.Client.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MonthDayPicker : BaseContentView
    {
        public Color SelectedColor { get; set; } = Color.Blue;

        private int _selectedDay = 1;
        public int SelectedDay
        {
            get { return _selectedDay; }
            set { _selectedDay = value; OnPropertyChanged(); }
        }

        public MonthDayPicker()
        {
            InitializeComponent();

            int day = 1;

            for (int row = 0; row < 5; ++row)
            {
                for (int col = 0; col < 7; ++col)
                {
                    if (day > 31)
                    {
                        break;
                    }

                    Frame frame = new Frame
                    {
                        BorderColor = Color.White,
                        HasShadow = false,
                        HeightRequest = 40,
                        WidthRequest = 40,
                        CornerRadius = 20,
                        Padding = new Thickness(0),
                        HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                        VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false)
                    };

                    TapGestureRecognizer frameTapGesture = new TapGestureRecognizer();
                    frameTapGesture.Tapped += FrameTapGesture_Tapped;

                    frame.GestureRecognizers.Add(frameTapGesture);

                    Label label = new Label
                    {
                        Text = day.ToString(GlobalSettings.Culture),
                        Margin = new Thickness(0),
                        Padding = new Thickness(0),
                        VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                        HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, false)
                    };

                    frame.Content = label;

                    Root.Children.Add(frame, col, row);

                    day++;
                }
            }

            Root.Children[SelectedDay - 1].BackgroundColor = SelectedColor;
        }

        private void FrameTapGesture_Tapped(object sender, EventArgs e)
        {
            Root.Children[SelectedDay - 1].BackgroundColor = Color.White;

            Frame frame = (Frame)sender;

            frame.BackgroundColor = SelectedColor;

            SelectedDay = ((Label)frame.Content).Text.ToInt32();

        }

        public override void OnAppearing()
        {
            base.OnAppearing();
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        public override IList<IBaseContentView?>? GetAllCustomerControls()
        {
            return null;
        }
    }
}