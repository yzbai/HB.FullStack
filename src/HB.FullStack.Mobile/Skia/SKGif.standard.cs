using System;
using System.IO;
using System.Threading.Tasks;

using HB.FullStack.Mobile.Platforms;

using SkiaSharp;

using Xamarin.Forms;

namespace HB.FullStack.Mobile.Skia
{
    public class SKGif : IDisposable
    {
        private SKBitmap[]? _bitmaps;
        private int[]? _durations;
        private int[]? _accumulatedDurations;
        private int _totalDuration;

        private readonly Task _initializeTask;

        public bool IsReady { get => _initializeTask != null && _initializeTask.IsCompletedSuccessfully; }

        public SKGif(string resourceName)
        {
            _initializeTask = InitializeAsync(resourceName);
            _initializeTask.Fire();
        }

        private Task InitializeAsync(string fileName)
        {
            return Task.Run(async () =>
            {
                IFileHelper fileService = DependencyService.Resolve<IFileHelper>();

                using Stream stream = await fileService.GetResourceStreamAsync(fileName, ResourceType.Drawable, null).ConfigureAwait(false);
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
            });
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
