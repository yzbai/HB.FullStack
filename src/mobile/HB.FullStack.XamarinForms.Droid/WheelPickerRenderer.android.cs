#nullable enable
using Android.Content;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using HB.FullStack.XamarinForms.Controls;

[assembly: ExportRenderer(typeof(HB.FullStack.XamarinForms.Controls.WheelPicker), typeof(HB.FullStack.XamarinForms.Droid.Renders.WheelPickerRenderer))]
namespace HB.FullStack.XamarinForms.Droid.Renders
{
    public class WheelPickerRenderer : ViewRenderer<WheelPicker, LinearLayout>
    {
        private bool _isDisposed = false;

        public WheelPickerRenderer(Context context) : base(context) { }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                _isDisposed = true;

                if (Element != null)
                {
                    UnRegisterCollectionChanged(Element);
                }
            }

            base.Dispose(disposing);
        }

        protected override LinearLayout CreateNativeControl()
        {
            LinearLayout layout = new LinearLayout(Context) { Orientation = Orientation.Horizontal };

            return layout;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<WheelPicker> e)
        {
            if (e.OldElement != null)
            {
                UnRegisterCollectionChanged(e.OldElement);
            }

            if (e.NewElement != null)
            {
                e.NewElement.ItemsSource.CollectionChanged += ItemsSource_CollectionChanged;

                if (Control == null)
                {
                    SetNativeControl(CreateNativeControl());
                }

                UpdateItemsSource();
                UpdateFont();
                UpdateDivider();
            }

            base.OnElementChanged(e);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == WheelPicker.ItemsSourceProperty.PropertyName)
            {
                UpdateItemsSource();
            }
            else if (e.PropertyName == WheelPicker.FontFamilyProperty.PropertyName ||
                e.PropertyName == WheelPicker.FontSizeProperty.PropertyName)
            {
                UpdateFont();
            }
            else if (e.PropertyName == WheelPicker.DividerColorProperty.PropertyName ||
                e.PropertyName == WheelPicker.DividerHeightProperty.PropertyName)
            {
                UpdateDivider();
            }
        }

        private void UpdateDivider()
        {
            if (Element.ItemsSource == null)
            {
                return;
            }

            for (int i = 0; i < Element.ItemsSource.Count; ++i)
            {
                if (Control.GetChildAt(i) is NumberPicker picker)
                {
                    SetNumberPickerDividerColor(picker, Element.DividerColor);
                    SetNumberPickerDividerHeight(picker, Element.DividerHeight);
                    //Util.SetNumberPickerWheelItemCount(picker, 5);
                }
            }
        }

        private void UpdateFont()
        {
            if (Element.ItemsSource == null)
            {
                return;
            }

            Font font = string.IsNullOrEmpty(Element.FontFamily) ?
                Font.SystemFontOfSize(Element.FontSize) :
                Font.OfSize(Element.FontFamily, Element.FontSize);

            float textSizeInSp = (float)(Element.FontSize * Context!.Resources!.DisplayMetrics!.Density);

            for (int i = 0; i < Element.ItemsSource.Count; ++i)
            {
                if (Control.GetChildAt(i) is NumberPicker picker)
                {
                    SetNumberPickerFont(picker, font.ToTypeface(), textSizeInSp);
                }
            }
        }

        private void UpdateItemsSource()
        {
            Control.RemoveAllViews();

            if (Element.ItemsSource == null)
            {
                return;
            }

            IList<bool> wrapWheels = Element.WrapSelectorWheels;

            if (wrapWheels == null || wrapWheels.Count != Element.ItemsSource.Count)
            {
                wrapWheels = new List<bool>();

                for (int i = 0; i < Element.ItemsSource.Count; ++i)
                {
                    wrapWheels.Add(true);
                }
            }

            for (int i = 0; i < Element.ItemsSource.Count; ++i)
            {
                NumberPicker picker = new NumberPicker(Context)
                {
                    LayoutParameters = new TableLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.MatchParent, 0.5f),
                    TextAlignment = Android.Views.TextAlignment.Center,
                    Tag = i,

                    MinValue = 0,
                    MaxValue = Element.ItemsSource[i].Count - 1
                };


                picker.WrapSelectorWheel = wrapWheels[i];
                picker.SetDisplayedValues(Element.ItemsSource[i].ToArray());


                #region restore Indexes

                int curIndex = Element.SelectedIndexes[i];

                if (curIndex < picker.MinValue)
                {
                    curIndex = picker.MinValue;
                }

                if (curIndex > picker.MaxValue)
                {
                    curIndex = picker.MaxValue;
                }

                Element.SelectedIndexes[i] = curIndex;

                picker.Value = curIndex;

                #endregion

                picker.ValueChanged += Picker_ValueChanged;

                Control.AddView(picker);
            }
        }

        private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //TODO:modify changed column
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    UpdateItemsSource();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    UpdateItemsSource();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    UpdateItemsSource();
                    break;
                case NotifyCollectionChangedAction.Move:
                    UpdateItemsSource();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    UpdateItemsSource();
                    break;
            }
        }

        private void Picker_ValueChanged(object sender, NumberPicker.ValueChangeEventArgs e)
        {
            if (Element.ItemsSource == null)
            {
                return;
            }

            int index = (int)e.Picker!.Tag!;

            Element.SelectedIndexes[index] = e.NewVal;
        }

        private void UnRegisterCollectionChanged(WheelPicker view)
        {
            if (Element.ItemsSource == null)
            {
                return;
            }

            Element.ItemsSource.CollectionChanged -= ItemsSource_CollectionChanged;
        }

        public static void SetNumberPickerFont(NumberPicker numberPicker, Typeface fontFamily, float textSizeInSp)
        {
            int count = numberPicker.ChildCount;

            for (int i = 0; i < count; i++)
            {
                Android.Views.View? child = numberPicker.GetChildAt(i);

                if (child is EditText editText)
                {
                    try
                    {
                        Java.Lang.Reflect.Field selectorWheelPaintField = numberPicker.Class
                            .GetDeclaredField("mSelectorWheelPaint");
                        selectorWheelPaintField.Accessible = true;
                        ((Paint)selectorWheelPaintField.Get(numberPicker)!).TextSize = textSizeInSp;
                        editText.Typeface = fontFamily;
                        editText.SetTextSize(Android.Util.ComplexUnitType.Px, textSizeInSp);
                        numberPicker.Invalidate();
                    }
                    catch (System.Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine("SetNumberPickerFont failed.", e);
                    }
                }
            }
        }

        public static void SetNumberPickerDividerColor(NumberPicker numberPicker, Xamarin.Forms.Color color)
        {
            try
            {
                Java.Lang.Reflect.Field field = numberPicker.Class.GetDeclaredField("mSelectionDivider");
                field.Accessible = true;
                field.Set(numberPicker, new ColorDrawable(color.ToAndroid()));
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("SetNumberPickerDividerColor failed.", e);
            }
        }

        public static void SetNumberPickerDividerHeight(NumberPicker numberPicker, int height)
        {
            try
            {
                Java.Lang.Reflect.Field field = numberPicker.Class.GetDeclaredField("mSelectionDividerHeight");
                field.Accessible = true;
                field.Set(numberPicker, height);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("SetNumberPickerDividerHeight failed.", e);
            }
        }
    }
}
#nullable restore