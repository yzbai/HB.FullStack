using System.ComponentModel;

namespace HB.FullStack.CommonTests.PropertyTrackable
{

    public class InnerTestObject : INotifyPropertyChanging, INotifyPropertyChanged
    {
        public event PropertyChangingEventHandler? PropertyChanging;
        public event PropertyChangedEventHandler? PropertyChanged;

        private string? _innerName;
        public string? InnerName
        {
            get => _innerName;
            set
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(InnerName)));

                _innerName = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InnerName)));
            }
        }

    }
}