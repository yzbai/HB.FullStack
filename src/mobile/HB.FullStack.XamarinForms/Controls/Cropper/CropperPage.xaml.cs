using HB.FullStack.XamarinForms.Base;
using HB.FullStack.XamarinForms.Effects.Touch;
using HB.FullStack.XamarinForms.Platforms;
using HB.FullStack.XamarinForms.Skia;

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

namespace HB.FullStack.XamarinForms.Controls.Cropper
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "已在Disappear中Dispose")]
    public partial class CropperPage : BaseContentPage
    {
        private readonly IFileHelper _fileHelper = DependencyService.Resolve<IFileHelper>();
        private readonly string _imageFullPath;
        private readonly string _croppedImageFullPath;
        private CropperFrameFigure? _cropperFrameFigure;
        private BitmapFigure? _bitmapFigure;

        public ICommand RotateCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand ResetCommand { get; }

        public ICommand CropCommand { get; }

        public ObservableRangeCollection<SKFigure> Figures { get; } = new ObservableRangeCollection<SKFigure>();

        public CropperPage(string imageFullPath, string croppedImageFullPath)
        {
            _imageFullPath = imageFullPath;
            _croppedImageFullPath = croppedImageFullPath;

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
            using FileStream stream = new FileStream(_imageFullPath, FileMode.Open);

            _bitmapFigure = new BitmapFigure(0.9f, 0.9f, stream)
            {
                EnableTwoFingers = true,
                ManipulationMode = TouchManipulationMode.IsotropicScale
            };

            _cropperFrameFigure = new CropperFrameFigure(0.7f, 0.7f, 0.9f, 0.9f);

            Figures.AddRange(new SKFigure[] { _bitmapFigure, _cropperFrameFigure });
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
            NavigationService.Current.Pop();
        }

        private async Task CropAsync()
        {
            if (_cropperFrameFigure == null || _bitmapFigure == null)
            {
                return;
            }

            using SKBitmap croppedBitmap = _bitmapFigure.Crop(_cropperFrameFigure.CropRect);

            await SaveSKBitmapAsync(croppedBitmap, _croppedImageFullPath).ConfigureAwait(false);

            NavigationService.Current.Pop();
        }

        private async Task SaveSKBitmapAsync(SKBitmap sKBitmap, string fullPath)
        {
            //Save
            using SKImage image = SKImage.FromBitmap(sKBitmap);

            using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);

            fullPath = Path.ChangeExtension(fullPath, ".png");

            await _fileHelper.SaveFileAsync(data.ToArray(), fullPath).ConfigureAwait(false);
        }
    }
}