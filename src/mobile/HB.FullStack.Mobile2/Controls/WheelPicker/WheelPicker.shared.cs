using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Controls
{
    public class WheelPicker : View
    {
        public ObservableCollection<int> SelectedIndexes { get; } = new ObservableCollection<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
            nameof(ItemsSource),
            typeof(ObservableCollection<IList<string>>),
            typeof(WheelPicker),
            new ObservableCollection<IList<string>>());

#pragma warning disable CA2227 // Collection properties should be read only
        public ObservableCollection<IList<string>> ItemsSource
#pragma warning restore CA2227 // Collection properties should be read only
        {
            get { return (ObservableCollection<IList<string>>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly BindableProperty WrapSelectorWheelsProperty = BindableProperty.Create(
            nameof(WrapSelectorWheels),
            typeof(IList<bool>),
            typeof(WheelPicker),
            null);

#pragma warning disable CA2227 // Collection properties should be read only
        public IList<bool> WrapSelectorWheels
#pragma warning restore CA2227 // Collection properties should be read only
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
            Color.LightGray);

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
