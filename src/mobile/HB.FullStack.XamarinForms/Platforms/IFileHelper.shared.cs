using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Platforms
{
    public enum ResourceType
    {
        Drawable
    }

    public interface IFileHelper
    {
        string GetDirectoryPath(UserFileType fileType);

        Task<Stream> GetResourceStreamAsync(string fileName, ResourceType resourceType, string? packageName = null, CancellationToken? cancellationToken = null);

        Task SaveFileAsync(byte[] data, string fileName, UserFileType userFileType);

        Task SaveFileAsync(byte[] data, string fullPath);

        Task SaveFileAsync(Stream stream, string fileName, UserFileType userFileType);

        bool IsFileExisted(string fileName, UserFileType avatar);

        Task<byte[]?> GetFileAsync(string fullPath);

        #region Avatar

        Task SaveAvatarAsync(ImageSource imageSource, long usreId);

        /// <summary>
        /// 如果不存在，返回null
        /// </summary>
        /// <returns></returns>
        string? GetAvatarFullPath(long userId);

        Task<byte[]?> GetAvatarAsync(long userId);
        
        Task SaveAvatarAsync(byte[] avatarData, long userId);

        Task SaveAvatarAsync(Stream stream, long userId);

        #endregion
    }

    public enum UserFileType
    {
        Avatar,
        Cache,
    }
}
