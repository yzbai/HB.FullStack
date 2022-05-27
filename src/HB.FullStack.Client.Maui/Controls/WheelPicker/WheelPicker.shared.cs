using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HB.FullStack.Client.Maui.Controls
{
    public class WheelPicker : View
    {
        public ObservableCollection<int> SelectedIndexes { get; } = new ObservableCollection<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
            nameof(ItemsSource),
            typeof(ObservableCollection<IList<string>>),
            typeof(WheelPicker),
            new ObservableCollection<IList<string>>());

        public ObservableCollection<IList<string>> ItemsSource
        {
            get { return (ObservableCollection<IList<string>>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly BindableProperty WrapSelectorWheelsProperty = BindableProperty.Create(
            nameof(WrapSelectorWheels),
            typeof(IList<bool>),
            typeof(WheelPicker),
            null);

        public IList<bool> WrapSelectorWheels
        {
            get { return (IList<bool>)GetValue(WrapSelectorWheelsProperty); }
            set { SetValue(WrapSelectorWheelsProperty, value); }
        }

        public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(
            nameof(FontSize),
            typeof(double),
            typeof(WheelPicker),
            -1.0,
            defaultValueCreator: bindable => Device.GetNamedSize(NamedSize.Default, (WheelPicker)bindable),
            coerceValue: CoerceFontSize);

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        private static object CoerceFontSize(BindableObject bindable, object value)
        {
            if (value == null)
            {
                return Device.GetNamedSize(NamedSize.Default, (WheelPicker)bindable);
            }
            return value;
        }

        public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create(
            nameof(FontFamily),
            typeof(string),
            typeof(WheelPicker),
            default(string));

        public string FontFamily
        {
            get { return (string)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public static readonly BindableProperty DividerColorProperty = BindableProperty.Create(
            nameof(DividerColor),
            typeof(Color),
            typeof(WheelPicker),
            Colors.LightGray);

        public Color DividerColor
        {
            get { return (Color)GetValue(DividerColorProperty); }
            set { SetValue(DividerColorProperty, value); }
        }

        public static readonly BindableProperty DividerHeightProperty = BindableProperty.Create(
            nameof(DividerHeight),
            typeof(int),
            typeof(WheelPicker),
            1);

        public int DividerHeight
        {
            get { return (int)GetValue(DividerHeightProperty); }
            set { SetValue(DividerHeightProperty, value); }
        }
    }
}
