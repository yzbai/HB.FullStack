/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using CommunityToolkit.Maui.Markup;

using HB.FullStack.Client.MauiLib.Base;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

using System;
using System.Collections.Generic;
using System.Linq;

namespace HB.FullStack.Client.MauiLib.Controls
{
    //TODO: 添加放大缩小动画
    public class SmsCodeEntry : BaseView
    {
        public static readonly BindableProperty CodeCountProperty = BindableProperty.Create(nameof(CodeCount), typeof(int), typeof(SmsCodeEntry), 6, BindingMode.OneWay, propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (bindable is SmsCodeEntry smsCodeEntry && newValue is int count)
            {
                smsCodeEntry.ResetLabels();
            }
        });

        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(SmsCodeEntry), "", BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
        {
            //TODO: 限制只能是数字
            if (bindable is SmsCodeEntry smsCodeEntry && newValue is string text)
            {
                smsCodeEntry.ResetLabelText();
            }
        });

        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(double), typeof(SmsCodeEntry), 16.0, propertyChanged: (b, o, n) =>
        {
            if (b is SmsCodeEntry smsCodeEntry && n is double size)
            {
                smsCodeEntry.ResetLabels();
            }
        });

        //public static readonly BindableProperty LabelWidthRequestProperty = BindableProperty.Create(nameof(LabelWidthRequest), typeof(double), typeof(SmsCodeEntry), 24.0, propertyChanged: (b, o, n) =>
        //{
        //    if (b is SmsCodeEntry smsCodeEntry && n is double width)
        //    {
        //        smsCodeEntry.ResetLabels();
        //    }
        //});
        public static readonly BindableProperty BorderUnSelectedColorProperty = BindableProperty.Create(nameof(BorderUnSelectedColor), typeof(Brush), typeof(SmsCodeEntry), Brush.DarkGreen, propertyChanged: (b, o, n) =>
        {
            if (b is SmsCodeEntry smsCodeEntry)
            {
                smsCodeEntry.ResetLabels();
            }
        });

        public static readonly BindableProperty BorderSelectedColorProperty = BindableProperty.Create(nameof(BorderSelectedColor), typeof(Brush), typeof(SmsCodeEntry), Brush.Green, propertyChanged: (b, o, n) =>
        {
            if (b is SmsCodeEntry smsCodeEntry)
            {
                smsCodeEntry.ResetLabels();
            }
        });

        public int CodeCount { get => (int)GetValue(CodeCountProperty); set => SetValue(CodeCountProperty, value); }
        public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
        public double FontSize { get => (double)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }

        //public double LabelWidthRequest { get => (double)GetValue(LabelWidthRequestProperty); set => SetValue(LabelWidthRequestProperty, value); }
        public Brush BorderSelectedColor { get => (Brush)GetValue(BorderSelectedColorProperty); set => SetValue(BorderSelectedColorProperty, value); }

        public Brush BorderUnSelectedColor { get => (Brush)GetValue(BorderUnSelectedColorProperty); set => SetValue(BorderUnSelectedColorProperty, value); }

        private readonly List<Border> _borders = new List<Border>();
        private readonly FlexLayout _pinLayout;
        private readonly Entry _inputEntry;

        public SmsCodeEntry()
        {
            //TODO: 尝试用Grid
            Content = new AbsoluteLayout
            {
                Children = {
                    new FlexLayout{ JustifyContent= FlexJustify.SpaceEvenly }
                        .Assign(out _pinLayout)
                        .Invoke(layout=>{
                            AbsoluteLayout.SetLayoutBounds(layout,new Rect(0,0,1,1));
                            AbsoluteLayout.SetLayoutFlags(layout, AbsoluteLayoutFlags.SizeProportional); }),

                    new Entry{ IsVisible = true, Opacity = 0, Keyboard= Keyboard.Numeric }
                        .Assign(out _inputEntry)
                        .Bind(Entry.MaxLengthProperty, nameof(CodeCount))
                        .Bind(Entry.TextProperty, nameof(Text))
                        .Invoke(layout=>{
                            AbsoluteLayout.SetLayoutBounds(layout,new Rect(0,0,1,1));
                            AbsoluteLayout.SetLayoutFlags(layout, AbsoluteLayoutFlags.SizeProportional); })
                }
            }.Invoke(layout => { layout.BindingContext = this; });

            ResetLabels();

            Focused += (sender, e) =>
            {
                _inputEntry.Focus();
                ResetLabelText();
            };

            Unfocused += (sender, e) =>
            {
                _inputEntry.Unfocus();
                ResetLabelText();
            };
        }

        public override void OnPageAppearing()
        {
            base.OnPageAppearing();
        }

        public override void OnPageDisappearing()
        {
            base.OnPageDisappearing();
        }

        private void ResetLabels()
        {
            _pinLayout.Children.Clear();
            _borders.Clear();

            //TODO:根据FontSize得到Label的大小
            HeightRequest = FontSize * 2;
            double labelWidth = FontSize + 10;

            for (int i = 0; i < CodeCount; ++i)
            {
                Border border = new Border
                {
                    Padding = new Thickness(0, 0, 0, 0),
                    Stroke = BorderUnSelectedColor,
                    StrokeThickness = 3,
                    StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(5, 5, 5, 5) },
                    Content = new Label
                    {
                        FontSize = FontSize,
                        WidthRequest = labelWidth,
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Center
                    }
                };

                _borders.Add(border);
                _pinLayout.Children.Add(border);
            }

            InvalidateMeasure();

            ResetLabelText();
        }

        private void ResetLabelText()
        {
            if (Text.IsNullOrEmpty())
            {
                _borders.ForEach(border =>
                {
                    ((Label)border.Content!).Text = " ";
                    border.Stroke = BorderUnSelectedColor;
                });

                if (IsFocused && _borders.Any())
                {
                    _borders[0].Stroke = BorderSelectedColor;
                }
            }
            else
            {
                if (Text.Length > CodeCount)
                {
                    Text = Text[..CodeCount];
                }

                int i = 0;

                for (; i < Text.Length; ++i)
                {
                    ((Label)_borders[i].Content!).Text = Text.Substring(i, 1);
                    _borders[i].Stroke = BorderUnSelectedColor;
                }

                for (; i < _borders.Count; ++i)
                {
                    ((Label)_borders[i].Content!).Text = " ";

                    _borders[i].Stroke = (IsFocused && i == Text.Length) ? BorderSelectedColor : BorderUnSelectedColor;
                }

                _inputEntry.CursorPosition = Text.IsNullOrEmpty() ? 0 : Text.Length;
            }
        }
    }
}