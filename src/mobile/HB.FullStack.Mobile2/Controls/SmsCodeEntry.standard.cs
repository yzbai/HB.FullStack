using AsyncAwaitBestPractices;

using HB.FullStack.XamarinForms.Base;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

using Xamarin.CommunityToolkit.Markup;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HB.FullStack.XamarinForms.Controls
{
    public class SmsCodeEntry : BaseContentView
    {
        public static readonly BindableProperty CodeCountProperty = BindableProperty.Create(nameof(CodeCount), typeof(int), typeof(SmsCodeEntry), 6, BindingMode.OneWay, propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (bindable is SmsCodeEntry smsCodeEntry && newValue is int count)
            {
                smsCodeEntry.ResetLabels(count);
            }
        });
        public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(SmsCodeEntry), "", BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (bindable is SmsCodeEntry smsCodeEntry && newValue is string text)
            {
                smsCodeEntry.ResetLabelText(text);
            }
        });
        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(double), typeof(SmsCodeEntry), 32.0);
        public static readonly BindableProperty LabelWidthRequestProperty = BindableProperty.Create(nameof(LabelWidthRequest), typeof(double), typeof(SmsCodeEntry), 18.0);
        public static readonly BindableProperty BorderUnSelectedColorProperty = BindableProperty.Create(nameof(BorderUnSelectedColor), typeof(Color), typeof(SmsCodeEntry), Color.DarkGreen, propertyChanged: (b, o, n) =>
        {
            if (b is SmsCodeEntry smsCodeEntry)
            {
                smsCodeEntry.ResetLabels(smsCodeEntry.CodeCount);
            }
        });
        public static readonly BindableProperty BorderSelectedColorProperty = BindableProperty.Create(nameof(BorderSelectedColor), typeof(Color), typeof(SmsCodeEntry), Color.Green, propertyChanged: (b, o, n) =>
        {
            if (b is SmsCodeEntry smsCodeEntry)
            {
                smsCodeEntry.ResetLabels(smsCodeEntry.CodeCount);
            }
        });

        public int CodeCount { get => (int)GetValue(CodeCountProperty); set => SetValue(CodeCountProperty, value); }
        public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
        public double FontSize { get => (double)GetValue(FontSizeProperty); set => SetValue(FontSizeProperty, value); }
        public double LabelWidthRequest { get => (double)GetValue(LabelWidthRequestProperty); set => SetValue(LabelWidthRequestProperty, value); }
        public Color BorderSelectedColor { get => (Color)GetValue(BorderSelectedColorProperty); set => SetValue(BorderSelectedColorProperty, value); }
        public Color BorderUnSelectedColor { get => (Color)GetValue(BorderUnSelectedColorProperty); set => SetValue(BorderUnSelectedColorProperty, value); }

        private readonly List<Frame> _frames = new List<Frame>();
        private FlexLayout _pinLayout;
        private Entry _inputEntry;

        public SmsCodeEntry()
        {
            Content = new StackLayout
            {
                Children = {
                new AbsoluteLayout { Children = {
                    new FlexLayout{ JustifyContent= FlexJustify.SpaceEvenly }
                        .Assign(out _pinLayout)
                        .Invoke(layout=>{
                            AbsoluteLayout.SetLayoutBounds(layout,new Rectangle(0,0,1,1));
                            AbsoluteLayout.SetLayoutFlags(layout, AbsoluteLayoutFlags.SizeProportional); }),

                    new Entry{ IsVisible = true, Opacity = 0, Keyboard= Keyboard.Numeric }
                        .Assign(out _inputEntry)
                        .Bind(Entry.MaxLengthProperty, nameof(CodeCount))
                        .Bind(Entry.TextProperty, nameof(Text))
                        .Invoke(layout=>{
                            AbsoluteLayout.SetLayoutBounds(layout,new Rectangle(0,0,1,1));
                            AbsoluteLayout.SetLayoutFlags(layout, AbsoluteLayoutFlags.SizeProportional); })
                    }}}
            }.Invoke(layout => { layout.BindingContext = this; });

            ResetLabels(CodeCount);
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

        public new void Focus()
        {
            _inputEntry.Focus();
        }

        private void ResetLabels(int count)
        {
            _pinLayout.Children.Clear();
            _frames.Clear();

            for (int i = 0; i < count; ++i)
            {
                Label label = new Label { FontSize = FontSize, WidthRequest = LabelWidthRequest };
                Frame frame = new Frame { Padding = new Thickness(10, 0, 10, 0), HasShadow = true, BorderColor = BorderUnSelectedColor };

                _frames.Add(frame);
                frame.Content = label;
                _pinLayout.Children.Add(frame);
            }
        }
        private void ResetLabelText(string text)
        {
            if (text.IsNullOrEmpty())
            {
                _frames.ForEach(f =>
                {
                    ((Label)f.Content).Text = "";
                    f.BorderColor = BorderUnSelectedColor;
                });

                if (_frames.Any())
                {
                    _frames[0].BorderColor = BorderSelectedColor;
                }
            }

            int i = 0;

            for (; i < text.Length; ++i)
            {
                ((Label)_frames[i].Content).Text = text.Substring(i, 1);
                _frames[i].BorderColor = BorderUnSelectedColor;
            }

            for (; i < _frames.Count; ++i)
            {
                ((Label)_frames[i].Content).Text = "";

                _frames[i].BorderColor = i == text.Length ? BorderSelectedColor : BorderUnSelectedColor;
            }

            //InputEntry Cursor Position
            _inputEntry.CursorPosition = text.IsNullOrEmpty() ? 0 : text.Length;
        }
    }


}