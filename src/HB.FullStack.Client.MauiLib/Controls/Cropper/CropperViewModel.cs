/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using HB.FullStack.Common;
using Microsoft.Maui.Controls;

using SkiaSharp;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using HB.FullStack.Client.MauiLib.Figures;
using HB.FullStack.Common.Files;
using HB.FullStack.Client.MauiLib.Utils;
using CommunityToolkit.Mvvm.Input;
using HB.FullStack.Client.Abstractions;
using HB.FullStack.Client.MauiLib.Base;

namespace HB.FullStack.Client.MauiLib.Controls
{
    [QueryProperty(nameof(ImageFullPath), nameof(ImageFullPath))]
    [QueryProperty(nameof(CroppedImageFullPath), nameof(CroppedImageFullPath))]
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Already taken care.")]
    public partial class CropperViewModel : BaseViewModel
    {
        private CropperFrameFigure? _cropperFrameFigure;
        private BitmapFigure? _bitmapFigure;

        public string? ImageFullPath { get; set; }
        public string? CroppedImageFullPath { get; set; }

        public ICommand RotateCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand ResetCommand { get; }

        public ICommand CropCommand { get; }

        public ObservableRangeCollection<SKFigure> Figures { get; } = new ObservableRangeCollection<SKFigure>();

        public CropperViewModel(
            ILogger<CropperViewModel> logger,
            IPreferenceProvider referenceProvider,
            IFileManager fileManager) : base(logger, referenceProvider, fileManager)
        {
            CropCommand = new AsyncRelayCommand(CropAsync);
            RotateCommand = new RelayCommand(Rotate);
            CancelCommand = new AsyncRelayCommand(CancelAsync);
            ResetCommand = new RelayCommand(Reset);
        }

        public override Task OnPageAppearingAsync()
        {
            IsBusy = true;

            ResumeFigures();

            IsBusy = false;

            return Task.CompletedTask;
        }

        public override Task OnPageDisappearingAsync()
        {
            RemoveFigures();
            return Task.CompletedTask;
        }

        private void ResumeFigures()
        {
            if (ImageFullPath.IsNullOrEmpty())
            {
                return;
            }

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
            if (CroppedImageFullPath.IsNullOrEmpty() || _cropperFrameFigure == null || _bitmapFigure == null)
            {
                return;
            }

            using SKBitmap croppedBitmap = _bitmapFigure.Crop(_cropperFrameFigure.CropRect);

            bool isSucceed = await SaveSKBitmapAsync(croppedBitmap, CroppedImageFullPath);

            await Currents.Shell.GoBackAsync(new Dictionary<string, object?> { { CropperPage.Query_CroppedSucceed, isSucceed } });
        }

        private static async Task<bool> SaveSKBitmapAsync(SKBitmap sKBitmap, string fullPathToSave)
        {
            //Save
            using SKImage image = SKImage.FromBitmap(sKBitmap);

            using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);

            fullPathToSave = Path.ChangeExtension(fullPathToSave, ".png");

            return await FileUtil.TrySaveFileAsync(data.ToArray(), fullPathToSave);
        }

        private void Rotate()
        {
            _bitmapFigure?.Rotate90(false);
        }

        private static async Task CancelAsync()
        {
            await Currents.Shell.GoBackAsync();
        }
    }
}