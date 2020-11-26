using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HB.FullStack.Client.Platforms
{
    public interface IPlatformFileHelper
    {
        Task SaveAvatarAsync(ImageSource imageSource, string usreGuid);

        Task<Stream> GetStreamOfResourceAsync(string fileName);

        /// <summary>
        /// 如果不存在，返回null
        /// </summary>
        /// <param name="userGuid"></param>
        /// <returns></returns>
        string? GetAvatarFilePath(string userGuid);
        Task<byte[]?> GetAvatarAsync(string userGuid);
    }
}
