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
using System.Collections.Generic;
using HB.FullStack.Client.MauiLib.Figures;
using HB.FullStack.Client.MauiLib.Utils;
using CommunityToolkit.Mvvm.Input;
using HB.FullStack.Client.MauiLib.Base;
using HB.FullStack.Common.Files;

namespace HB.FullStack.Client.MauiLib.Controls
{
    [QueryProperty(nameof(ImageFullPath), nameof(ImageFullPath))]
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Already taken care.")]
    public partial class CropperViewModel : BaseViewModel
    {
        private CropperFrameFigure? _cropperFrameFigure;
        private BitmapFigure? _bitmapFigure;
        private readonly ILocalFileManager _localFileManager;

        public string? ImageFullPath { get; set; }

        public ObservableRangeCollection<SKFigure> Figures { get; } = new ObservableRangeCollection<SKFigure>();

        public CropperViewModel(ILocalFileManager localFileManager)
        {
            _localFileManager = localFileManager;
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

        [RelayCommand]
        private void Reset()
        {
            RemoveFigures();
            ResumeFigures();
        }

        [RelayCommand]
        private async Task CropAsync()
        {
            if (_cropperFrameFigure == null || _bitmapFigure == null)
            {
                return;
            }

            using SKBitmap croppedBitmap = _bitmapFigure.Crop(_cropperFrameFigure.CropRect);

            string croppedFullPath = _localFileManager.GetNewTempFullPath(".png");

            bool isSucceed = await SKUtil.SaveSKBitmapAsync(croppedBitmap, croppedFullPath);

            await Currents.Shell.GoBackAsync(new Dictionary<string, object?>
            {
                { CropperPage.Query_CroppedSuccess, isSucceed },
                { CropperPage.Query_CroppedFullPath, croppedFullPath}
            });
        }

        [RelayCommand]
        private void Rotate()
        {
            _bitmapFigure?.Rotate90(false);
        }

        [RelayCommand]
        private static async Task CancelAsync()
        {
            await Currents.Shell.GoBackAsync();
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
    }
}