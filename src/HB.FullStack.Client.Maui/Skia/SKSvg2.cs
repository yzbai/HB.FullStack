using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SkiaSharp;

using Svg.Skia;

namespace HB.FullStack.Client.Maui.Skia
{
    public class SKSvg2 : IDisposable
    {
        private readonly SKSvg _svg;
        private bool _disposedValue;

        public bool IsReady { get; private set; }

        public SKSvg2()
        {
            _svg = new SKSvg();
        }

        public void Load(Stream stream)
        {
            IsReady = false;

            _svg.Load(stream);

            IsReady = true;
        }

        public SKPicture? GetSKPicture()
        {
            if (IsReady)
            {
                return _svg.Picture;
            }

            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    IsReady = false;
                    _svg.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SKSvg2()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
