using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace HB.FullStack.Mobile.Platforms
{
    public interface IFileHelper
    {
        string GetDirectoryPath(UserFileType fileType);

        Task<Stream> GetResourceStreamAsync(string fileName);

        Task SaveFileAsync(byte[] data, string fileName, UserFileType userFileType);


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
