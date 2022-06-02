using AsyncAwaitBestPractices.MVVM;

using HB.FullStack.Client.Maui.Base;
using HB.FullStack.Client.Maui.Figures;
using HB.FullStack.Common;

using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Toolkit.Mvvm.Input;

using SkiaSharp;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HB.FullStack.Client.Maui.Controls.Cropper
{
    [QueryProperty(nameof(ImageFullPath), nameof(ImageFullPath))]
    [QueryProperty(nameof(CroppedImageFullPath), nameof(CroppedImageFullPath))]
    public class CropperViewModel : BaseViewModel
    {

        //private readonly Action<bool> _onCroppFinish;
        private CropperFrameFigure? _cropperFrameFigure;
        private BitmapFigure? _bitmapFigure;

        /// <summary>
        /// 本地原始图片路径
        /// </summary>
        public string? ImageFullPath { get; set; }

        /// <summary>
        /// 剪切后的存储位置
        /// </summary>
        public string? CroppedImageFullPath { get; set; }

        public ICommand RotateCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand ResetCommand { get; }

        public ICommand CropCommand { get; }

        public ObservableRangeCollection<SKFigure> Figures { get; } = new ObservableRangeCollection<SKFigure>();

        public CropperViewModel(ILogger<CropperViewModel> logger/*string originalImageFullPath, string croppedImageFullPath, Action<bool> onCroppFinish*/) : base(logger)
        {
            CropCommand = new AsyncCommand(CropAsync);
            RotateCommand = new Command(Rotate);
            CancelCommand = new AsyncCommand(CancelAsync);
            ResetCommand = new Command(Reset);
        }

        public override Task OnPageAppearingAsync()
        {
            IsBusy = true;

            ResumeFigures();

            IsBusy = false;

            return base.OnPageAppearingAsync();
        }

        public override Task OnPageDisappearingAsync()
        {
            RemoveFigures();
            return base.OnPageDisappearingAsync();
        }

        private void ResumeFigures()
        {
            using FileStream stream = new FileStream(ImageFullPath, FileMode.Open);

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
            foreach (SKFigure figure in Figures)
            {
                figure.Dispose();
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

        private async Task CropAsync()
        {
            if (_cropperFrameFigure == null || _bitmapFigure == null)
            {
                return;
            }

            using SKBitmap croppedBitmap = _bitmapFigure.Crop(_cropperFrameFigure.CropRect);

            bool isSucceed = await SaveSKBitmapAsync(croppedBitmap, CroppedImageFullPath).ConfigureAwait(false);

            //_onCroppFinish(isSucceed);

            await INavigationManager.Current.GotoAsync($"..?IsCropSucceed={isSucceed}").ConfigureAwait(false);
        }

        private static async Task<bool> SaveSKBitmapAsync(SKBitmap sKBitmap, string fullPathToSave)
        {
            //Save
            using SKImage image = SKImage.FromBitmap(sKBitmap);

            using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);

            fullPathToSave = Path.ChangeExtension(fullPathToSave, ".png");

            return await FileUtil.TrySaveFileAsync(data.ToArray(), fullPathToSave).ConfigureAwait(false);
        }

        private void Rotate()
        {
            _bitmapFigure?.Rotate90(false);
        }

        private static async Task CancelAsync()
        {
            await INavigationManager.Current!.GoBackAsync().ConfigureAwait(false);
        }
    }
}