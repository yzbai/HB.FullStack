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

        bool IsFileExisted(string fileName, UserFileType avatar);

        #region Avatar

        Task SaveAvatarAsync(ImageSource imageSource, long usreId);

        /// <summary>
        /// 如果不存在，返回null
        /// </summary>
        /// <returns></returns>
        string? GetAvatarFilePath(long userId);

        Task<byte[]?> GetAvatarAsync(long userId);

        #endregion
    }

    public enum UserFileType
    {
        Avatar,
    }
}
