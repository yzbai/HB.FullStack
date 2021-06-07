using System;
using System.Collections.Generic;
using System.Globalization;

using HB.FullStack.XamarinForms.Base;

using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using static Xamarin.CommunityToolkit.Markup.GridRowsColumns;

namespace HB.FullStack.XamarinForms.Controls
{
    public class MonthDayPicker : BaseContentView
    {
        public Color SelectedColor { get; set; } = Color.Blue;

        private int _selectedDay = 1;
        public int SelectedDay
        {
            get { return _selectedDay; }
            set { _selectedDay = value; OnPropertyChanged(); }
        }

        private Grid _root;

        public MonthDayPicker()
        {
            Content = new Grid
            {
                RowDefinitions = Rows.Define(Auto, Auto, Auto, Auto, Auto),
                ColumnDefinitions = Columns.Define(Star, Star, Star, Star, Star, Star, Star)
            }.Assign(out _root);

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

                    _root.Children.Add(frame, col, row);

                    day++;
                }
            }

            _root.Children[SelectedDay - 1].BackgroundColor = SelectedColor;
        }

        private void FrameTapGesture_Tapped(object? sender, EventArgs e)
        {
            _root.Children[SelectedDay - 1].BackgroundColor = Color.White;

            if (sender is Frame frame)
            {
                frame.BackgroundColor = SelectedColor;

                SelectedDay = Convert.ToInt32(((Label)frame.Content).Text, CultureInfo.InvariantCulture);
            }
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