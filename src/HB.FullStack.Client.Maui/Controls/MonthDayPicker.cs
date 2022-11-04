using System;
using System.Globalization;

using CommunityToolkit.Maui.Markup;

using HB.FullStack.Client.Maui.Base;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace HB.FullStack.Client.Maui.Controls
{
    public class MonthDayPicker : BaseView
    {
        public Color SelectedColor { get; set; } = Colors.Blue;

        private int _selectedDay = 1;
        public int SelectedDay
        {
            get { return _selectedDay; }
            set { _selectedDay = value; OnPropertyChanged(); }
        }

        private readonly Grid _root;

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
                        BorderColor = Colors.White,
                        HasShadow = false,
                        HeightRequest = 40,
                        WidthRequest = 40,
                        CornerRadius = 20,
                        Padding = new Thickness(0),
                        HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                        VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false)
                    }.Row(row).Column(col);

                    TapGestureRecognizer frameTapGesture = new TapGestureRecognizer();
                    frameTapGesture.Tapped += FrameTapGesture_Tapped;

                    frame.GestureRecognizers.Add(frameTapGesture);

                    Label label = new Label
                    {
                        Text = day.ToString(Globals.Culture),
                        Margin = new Thickness(0),
                        Padding = new Thickness(0),
                        VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                        HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, false)
                    };

                    frame.Content = label;

                    _root.Children.Add(frame/*, col, row*/);

                    day++;
                }
            }

            (_root.Children[SelectedDay - 1] as Frame)!.BackgroundColor = SelectedColor;
        }

        private void FrameTapGesture_Tapped(object? sender, EventArgs e)
        {
            (_root.Children[SelectedDay - 1] as Frame)!.BackgroundColor = Colors.White;

            if (sender is Frame frame)
            {
                frame.BackgroundColor = SelectedColor;

                SelectedDay = Convert.ToInt32(((Label)frame.Content).Text, CultureInfo.InvariantCulture);
            }
        }

        public override void OnPageAppearing()
        {
            base.OnPageAppearing();
        }

        public override void OnPageDisappearing()
        {
            base.OnPageDisappearing();
        }
    }
}