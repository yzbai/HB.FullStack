using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HB.Framework.Client.Platforms
{
    public interface IPlatformFileHelper
    {
        Task SaveUserHeadImageAsync(ImageSource imageSource, string usreGuid);

        Task<Stream> GetStreamOfResourceAsync(string fileName);

    }
}
