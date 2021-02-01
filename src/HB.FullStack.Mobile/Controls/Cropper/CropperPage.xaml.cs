using HB.FullStack.Mobile.Base;
using HB.FullStack.Mobile.Effects.Touch;
using HB.FullStack.Mobile.Platforms;
using HB.FullStack.Mobile.Skia;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HB.FullStack.Mobile.Controls.Cropper
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "已在Disappear中Dispose")]
    public partial class CropperPage : BaseContentPage
    {
        private readonly IFileHelper _fileHelper = DependencyService.Resolve<IFileHelper>();
        private readonly string _resourceName;
        private readonly string _fileNameWithoutSuffix;
        private readonly UserFileType _userFileType;
        private CropperFrameFigure? _cropperFrameFigure;
        private BitmapFigure? _bitmapFigure;

        public ICommand RotateCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand ResetCommand { get; }

        public ICommand CropCommand { get; }

        public ObservableRangeCollection<SKFigure> Figures { get; } = new ObservableRangeCollection<SKFigure>();

        public CropperPage(string resourceName, string fileNameWithoutSuffix, UserFileType userFileType)
        {
            _resourceName = resourceName;
            _fileNameWithoutSuffix = fileNameWithoutSuffix;
            _userFileType = userFileType;

            InitializeComponent();

            CropCommand = new AsyncCommand(CropAsync, onException: GlobalSettings.ExceptionHandler);
            RotateCommand = new Command(Rotate);
            CancelCommand = new Command(Cancel);
            ResetCommand = new Command(Reset);

            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ResumeFigures();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            RemoveFigures();
        }

        protected override IList<IBaseContentView?>? GetAllCustomerControls()
            => new List<IBaseContentView?> { FigureCanvas };

        private void ResumeFigures()
        {
            _bitmapFigure = new BitmapFigure(0.9f, 0.9f, null)
            {
                EnableTwoFingers = true,
                ManipulationMode = TouchManipulationMode.IsotropicScale
            };

            _cropperFrameFigure = new CropperFrameFigure(0.7f, 0.7f, 0.9f, 0.9f);

            Figures.AddRange(new SKFigure[] { _bitmapFigure, _cropperFrameFigure });

            _fileHelper.GetResourceStreamAsync(_resourceName).ContinueWith(async streamTask =>
            {
                using Stream stream = await streamTask.ConfigureAwait(false);

                _bitmapFigure.SetBitmap(stream);

            }, TaskScheduler.Default).Fire();
        }

        private void RemoveFigures()
        {
            foreach (SKFigure f in Figures)
            {
                f.Dispose();
            }

            Figures.Clear();

            _cropperFrameFigure = null;
            _bitmapFigure = null;
        }

        private void Reset()
        {
            RemoveFigures();
            ResumeFigures();
        }

        private void Rotate()
        {
            _bitmapFigure?.Rotate90(false);
        }

        private void Cancel()
        {
            Pop();
        }

        private async Task CropAsync()
        {
            if (_cropperFrameFigure == null || _bitmapFigure == null)
            {
                return;
            }

            SKRect cropRect = _cropperFrameFigure.CropRect;

            using SKBitmap croppedBitmap = _bitmapFigure.Crop(cropRect);

            //Save
            using SKImage image = SKImage.FromBitmap(croppedBitmap);

            using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);

            string fileName = _fileNameWithoutSuffix.Contains('.', StringComparison.InvariantCulture) ? _fileNameWithoutSuffix : _fileNameWithoutSuffix + ".png";

            await _fileHelper.SaveFileAsync(data.ToArray(), fileName, _userFileType).ConfigureAwait(false);

            Pop();
        }
    }
}