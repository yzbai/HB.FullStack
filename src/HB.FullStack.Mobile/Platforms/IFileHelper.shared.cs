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
        Task SaveAvatarAsync(ImageSource imageSource, long usreId);

        Task<Stream> GetStreamOfResourceAsync(string fileName);

        /// <summary>
        /// 如果不存在，返回null
        /// </summary>
        /// <returns></returns>
        string? GetAvatarFilePath(long userId);
        Task<byte[]?> GetAvatarAsync(long userId);
    }
}
