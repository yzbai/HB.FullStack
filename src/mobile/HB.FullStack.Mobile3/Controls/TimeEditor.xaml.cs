using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using HB.FullStack.Common;
using HB.FullStack.Mobile.Base;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HB.FullStack.Mobile.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TimeEditor : BaseContentView
    {
        public static readonly BindableProperty IsDisplay24HourFormatProperty = BindableProperty.Create(nameof(IsDisplay24HourFormat), typeof(bool), typeof(TimeEditor), false, propertyChanged: (b, o, n) => ((TimeEditor)b).OnIsDisplay24HourFormatChanged((bool)o, (bool)n));
        public static readonly BindableProperty TimeProperty = BindableProperty.Create(nameof(Time), typeof(Time24Hour), typeof(TimeEditor), propertyChanged: (b, o, n) => ((TimeEditor)b).OnTimeChanged((Time24Hour)o, (Time24Hour)n));

        public bool IsDisplay24HourFormat
        {
            get => (bool)GetValue(IsDisplay24HourFormatProperty);
            set => SetValue(IsDisplay24HourFormatProperty, value);
        }

        public Time24Hour Time
        {
            get => (Time24Hour)GetValue(TimeProperty);
            set
            {
                SetValue(TimeProperty, value);
                OnPropertyChanged(nameof(Hour));
                OnPropertyChanged(nameof(Minute));
                OnPropertyChanged(nameof(AmPm));
            }
        }

        public string Hour => (IsDisplay24HourFormat ? Time.Hour : (Time.Hour > 12 ? Time.Hour - 12 : Time.Hour)).ToString("D2", GlobalSettings.Culture);

        public string Minute => Time.Minute.ToString(GlobalSettings.Culture);

        public string AmPm => Time.IsAm ? "上午" : "下午";

        public ICommand AmPmChangedCommand { get; set; }

        public ICommand HourChangedCommand { get; set; }

        public ICommand MinuteChangedCommand { get; set; }

        public ICommand TestCommand { get; set; }

        public TimeEditor()
        {


            AmPmChangedCommand = new Command<object>(OnAmPmChanged);
            HourChangedCommand = new Command<object>(OnHourChanged);
            MinuteChangedCommand = new Command<object>(OnMinuteChanged);


            TestCommand = new Command(() =>
            {


            });


            //如果 放到Command声明之前，则要OnPropertyChanged通知Command发生改变
            InitializeComponent();

            //OnPropertyChanged(nameof(AmPmChangedCommand));
            //OnPropertyChanged(nameof(HourChangedCommand));
            //OnPropertyChanged(nameof(MinuteChangedCommand));
        }

        private void OnMinuteChanged(object obj)
        {
            bool isUp = Convert.ToBoolean(obj, GlobalSettings.Culture);
            Time = Time.AddTime(0, isUp ? 1 : -1);
        }

        private void OnHourChanged(object obj)
        {
            bool isUp = Convert.ToBoolean(obj, GlobalSettings.Culture);
            Time = Time.AddTime(isUp ? 1 : -1, 0);
        }

        private void OnAmPmChanged(object obj)
        {
            bool isUp = Convert.ToBoolean(obj, GlobalSettings.Culture);
            Time = Time.AddTime(Time.IsAm ? 12 : -12, 0);
        }

        public override IList<IBaseContentView?>? GetAllCustomerControls() => null;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "<Pending>")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        private void OnIsDisplay24HourFormatChanged(bool oldValue, object newValue)
        {

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "<Pending>")]
        private void OnTimeChanged(Time24Hour oldValue, Time24Hour newValue)
        {
            OnPropertyChanged(nameof(Hour));
            OnPropertyChanged(nameof(Minute));
            OnPropertyChanged(nameof(AmPm));
        }
    }
}