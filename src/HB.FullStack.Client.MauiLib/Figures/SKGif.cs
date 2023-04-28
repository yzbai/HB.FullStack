using System;
using System.IO;

using Microsoft.Extensions.Logging;

using SkiaSharp;

namespace HB.FullStack.Client.MauiLib.Figures
{
    public class SKGif : IDisposable
    {
        private SKBitmap[]? _bitmaps;
        private int[]? _durations;
        private int[]? _accumulatedDurations;
        private int _totalDuration;

        public bool IsReady { get; private set; }

        public SKGif()
        {
        }

        public void Load(Stream stream)
        {
            IsReady = false;

            using SKManagedStream sKManagedStream = new SKManagedStream(stream);
            using SKCodec sKCodec = SKCodec.Create(sKManagedStream);

            int frameCount = sKCodec.FrameCount;
            _bitmaps = new SKBitmap[frameCount];
            _durations = new int[frameCount];
            _accumulatedDurations = new int[frameCount];

            for (int frame = 0; frame < frameCount; frame++)
            {
                //get time line
                _durations[frame] = sKCodec.FrameInfo[frame].Duration;
                _totalDuration += _durations[frame];
                _accumulatedDurations[frame] = _durations[frame] + (frame == 0 ? 0 : _accumulatedDurations[frame - 1]);

                //get image
                SKImageInfo sKImageInfo = new SKImageInfo(sKCodec.Info.Width, sKCodec.Info.Height);
                _bitmaps[frame] = new SKBitmap(sKImageInfo);

                IntPtr pointer = _bitmaps[frame].GetPixels();

                sKCodec.GetPixels(sKImageInfo, pointer, new SKCodecOptions(frame));
            }

            IsReady = true;

            Globals.Logger.LogDebug("SkGif已经加载完毕");
        }

        public SKBitmap GetBitmap(long elapsedMilliseconds)
        {
            

            int msec = (int)(elapsedMilliseconds % _totalDuration);
            int frame;

            for (frame = 0; frame < _accumulatedDurations!.Length; frame++)
            {
                if (msec < _accumulatedDurations[frame])
                {
                    break;
                }
            }

            //Globals.Logger.LogDebug("SKGif 获取 第 {frame} frame 图像", frame);

            return _bitmaps![frame];
        }

        #region Dispose Pattern

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposeManaged)
        {
            if (!_disposed)
            {
                if (disposeManaged)
                {
                    IsReady = false;

                    // managed
                    if (_bitmaps.IsNotNullOrEmpty())
                    {
                        foreach (var bitmap in _bitmaps)
                        {
                            bitmap?.Dispose();
                        }
                    }
                }

                //unmanaged

                _disposed = true;
            }
        }

        #endregion
    }
}
