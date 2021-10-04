#nullable disable
using HB.FullStack.Common.Api;
using HB.FullStack.XamarinForms.Api;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


using Xamarin.Forms;
using Xamarin.Forms.Internals;

using IOPath = System.IO.Path;

namespace HB.FullStack.XamarinForms.Controls
{
    /// <summary>
    /// 从Xamarin.Forms复制. 
    /// </summary>
    public sealed class AuthUriImageSource : ImageSource
    {
        internal const string CacheName = "ImageLoaderCache";

        public static readonly BindableProperty UriProperty = BindableProperty.Create(nameof(Uri), typeof(Uri), typeof(AuthUriImageSource), default(Uri),
            propertyChanged: (bindable, oldvalue, newvalue) => ((AuthUriImageSource)bindable).OnUriChanged(), validateValue: (bindable, value) => value == null || ((Uri)value).IsAbsoluteUri);

        static readonly Xamarin.Forms.Internals.IIsolatedStorageFile Store = Device.PlatformServices.GetUserStoreForApplication();

        //static readonly object s_syncHandle = new object();
        static readonly ConcurrentDictionary<string, LockingSemaphore> s_semaphores = new ConcurrentDictionary<string, LockingSemaphore>();

        TimeSpan _cacheValidity = TimeSpan.FromDays(1);

        bool _cachingEnabled = true;

        static AuthUriImageSource()
        {
            if (!Store.GetDirectoryExistsAsync(CacheName).Result)
                Store.CreateDirectoryAsync(CacheName).Wait();
        }

        public override bool IsEmpty => Uri == null;

        public TimeSpan CacheValidity
        {
            get { return _cacheValidity; }
            set
            {
                if (_cacheValidity == value)
                    return;

                OnPropertyChanging();
                _cacheValidity = value;
                OnPropertyChanged();
            }
        }

        public bool CachingEnabled
        {
            get { return _cachingEnabled; }
            set
            {
                if (_cachingEnabled == value)
                    return;

                OnPropertyChanging();
                _cachingEnabled = value;
                OnPropertyChanged();
            }
        }

        [Xamarin.Forms.TypeConverter(typeof(Xamarin.Forms.UriTypeConverter))]
        public Uri Uri
        {
            get { return (Uri)GetValue(UriProperty); }
            set { SetValue(UriProperty, value); }
        }

        /// <summary>
        /// GetStreamAsync
        /// </summary>
        /// <param name="userToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="Exception"></exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public async Task<Stream> GetStreamAsync(CancellationToken userToken = default(CancellationToken))
        {
            OnLoadingStarted();
            userToken.Register(CancellationTokenSource.Cancel);
            Stream stream;

            try
            {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                stream = await GetStreamAsync(Uri, CancellationTokenSource.Token);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
                OnLoadingCompleted(false);
            }
            catch (OperationCanceledException)
            {
                OnLoadingCompleted(true);
                throw;
            }
            catch (Exception ex)
            {
                GlobalSettings.Logger.LogError("Image Loading", $"Error getting stream for {Uri}: {ex}");
                throw;
            }

            return stream;
        }

        public override string ToString()
        {
            return $"Uri: {Uri}";
        }

        static string GetCacheKey(Uri uri)
        {
            return Device.PlatformServices.GetHash(uri.AbsoluteUri);
        }

#pragma warning disable CA1801 // Review unused parameters
        async Task<bool> GetHasLocallyCachedCopyAsync(string key, bool checkValidity = true)
#pragma warning restore CA1801 // Review unused parameters
        {
            DateTime now = DateTime.UtcNow;
            DateTime? lastWriteTime = await GetLastWriteTimeUtcAsync(key).ConfigureAwait(false);
            return lastWriteTime.HasValue && now - lastWriteTime.Value < CacheValidity;
        }

        static async Task<DateTime?> GetLastWriteTimeUtcAsync(string key)
        {
            string path = IOPath.Combine(CacheName, key);
            if (!await Store.GetFileExistsAsync(path).ConfigureAwait(false))
                return null;

            return (await Store.GetLastWriteTimeAsync(path).ConfigureAwait(false)).UtcDateTime;
        }

        /// <summary>
        /// GetStreamAsync
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        async Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            Stream stream = null;

            if (CachingEnabled)
                stream = await GetStreamFromCacheAsync(uri, cancellationToken).ConfigureAwait(false);

            if (stream == null)
            {
                try
                {
                    stream = await GetStreamCoreAsync(uri, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Xamarin.Forms.Internals.Log.Warning("Image Loading", $"Error getting stream for {Uri}: {ex}");
                    stream = null;
                }
            }

            return stream;
        }

        async Task<Stream> GetStreamAsyncUnchecked(string key, Uri uri, CancellationToken cancellationToken)
        {
            if (await GetHasLocallyCachedCopyAsync(key).ConfigureAwait(false))
            {
                var retry = 5;
                while (retry >= 0)
                {
                    int backoff;
                    try
                    {
                        Stream result = await Store.OpenFileAsync(IOPath.Combine(CacheName, key), FileMode.Open, FileAccess.Read).ConfigureAwait(false);
                        return result;
                    }
                    catch (IOException)
                    {
                        // iOS seems to not like 2 readers opening the file at the exact same time, back off for random amount of time
#pragma warning disable CA5394 // Do not use insecure randomness
                        backoff = new Random().Next(1, 5);
#pragma warning restore CA5394 // Do not use insecure randomness
                        retry--;
                    }

                    if (backoff > 0)
                    {
#pragma warning disable CA2016 // Forward the 'CancellationToken' parameter to methods that take one
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                        await Task.Delay(backoff);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
#pragma warning restore CA2016 // Forward the 'CancellationToken' parameter to methods that take one
                    }
                }
                return null;
            }

            Stream stream;
            try
            {
                stream = await GetStreamCoreAsync(uri, cancellationToken).ConfigureAwait(false);
                if (stream == null)
                    return null;
            }
            catch (Exception ex)
            {
                Log.Warning("Image Loading", $"Error getting stream for {Uri}: {ex}");
                return null;
            }

            if (stream == null || !stream.CanRead)
            {
                stream?.Dispose();
                return null;
            }

            try
            {
                Stream writeStream = await Store.OpenFileAsync(IOPath.Combine(CacheName, key), FileMode.Create, FileAccess.Write).ConfigureAwait(false);
                await stream.CopyToAsync(writeStream, 16384, cancellationToken).ConfigureAwait(false);
                if (writeStream != null)
                    writeStream.Dispose();

                stream.Dispose();

                return await Store.OpenFileAsync(IOPath.Combine(CacheName, key), FileMode.Open, FileAccess.Read).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Warning("Image Loading", $"Error getting stream for {Uri}: {ex}");
                return null;
            }
        }

        /// <summary>
        /// GetStreamFromCacheAsync
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="OperationCanceledException"></exception>
        async Task<Stream> GetStreamFromCacheAsync(Uri uri, CancellationToken cancellationToken)
        {
            string key = GetCacheKey(uri);
            LockingSemaphore sem = s_semaphores.GetOrAdd(key, _=>new LockingSemaphore(1));
            //lock (s_syncHandle)
            //{
            //    if (s_semaphores.ContainsKey(key))
            //        sem = s_semaphores[key];
            //    else
            //        s_semaphores.Add(key, sem = new LockingSemaphore(1));
            //}

            try
            {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                await sem.WaitAsync(cancellationToken);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
                Stream stream = await GetStreamAsyncUnchecked(key, uri, cancellationToken);
#pragma warning restore CA2007 // Consider calling ConfigureAwait on the awaited task
                if (stream == null || stream.Length == 0 || !stream.CanRead)
                {
                    sem.Release();
                    return null;
                }
                var wrapped = new WrappedStream(stream);
                wrapped.Disposed += (o, e) => sem.Release();
                return wrapped;
            }
            catch (OperationCanceledException)
            {
                sem.Release();
                throw;
            }
        }

        void OnUriChanged()
        {
            if (CancellationTokenSource != null)
                CancellationTokenSource.Cancel();
            OnSourceChanged();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "<Pending>")]
        public static Task<Stream> GetStreamCoreAsync(Uri uri, CancellationToken cancellationToken)
        {
            //if (ApiClient == null)
            //{
            //    using (var client = new HttpClient())
            //    {
            //        // Do not remove this await otherwise the client will dispose before
            //        // the stream even starts
            //        var result = await StreamWrapper.GetStreamAsync(uri, cancellationToken, client).ConfigureAwait(false);

            //        return result;
            //    }
            //}
            //else
            //{
            //    using (var client = new HttpClient(HttpClientHandler))
            //    {
            //        // Do not remove this await otherwise the client will dispose before
            //        // the stream even starts
            //        var result = await StreamWrapper.GetStreamAsync(uri, cancellationToken, client).ConfigureAwait(false);

            //        return result;
            //    }
            //}

            return ApiClient.GetStreamAsync(new ImageUrlRequest(uri.AbsoluteUri));

        }

        private static IApiClient _apiClient;

        public static IApiClient ApiClient
        {
            get
            {
                if (_apiClient == null)
                {
                    _apiClient = DependencyService.Resolve<IApiClient>();
                }

                return _apiClient;
            }
        }
    }

    internal class ImageUrlRequest : ApiRequest
    {
        private readonly string _uri;

        public ImageUrlRequest(string uri) : base(HttpMethod.Get, ApiAuthType.Jwt, null, null, null, null,null)
        {
            _uri = uri;
        }

        public override string ToDebugInfo()
        {
            return $"ImageUrlRequest Uri:{_uri}";
        }

        protected override string BuildUrl()
        {
            return _uri;
        }
    }
}
#nullable restore