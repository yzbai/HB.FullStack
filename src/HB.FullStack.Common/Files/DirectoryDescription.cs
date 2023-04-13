using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Files
{

    public class DirectoryDescription
    {
        public string DirectoryName { get; set; } = null!;

        public string DirectoryPath { get; set; } = null!;

        public string DirectoryPermissionName { get; set; } = null!;

        /// <summary>
        /// 客户端根据此来判断本地文件是否过期,跟服务端无关
        /// </summary>
        public TimeSpan ExpiryTime { get; set; } = TimeSpan.FromHours(1);

        /// <summary>
        /// 即是否包含PlaceHolder
        /// </summary>
        public bool IsPathContainsPlaceHolder{ get; set; }

        //TODO:考虑多个PlaceHolder
        public string? PlaceHolderName { get; set; }
    }
}
