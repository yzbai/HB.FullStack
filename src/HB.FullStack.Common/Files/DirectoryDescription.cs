using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Files
{

    public class DirectoryDescription
    {
        public static string UserPlaceHolder { get; } = "{User}";

        public string DirectoryName { get; set; } = null!;

        public string DirectoryPath { get; set; } = null!;

        /// <summary>
        /// 即是否包含UserPlaceHolder,只能本用户读取
        /// </summary>
        public bool IsUserPrivate { get; set; } = true;

        public string DirectoryPermissionName { get; set; } = null!;

        /// <summary>
        /// 客户端根据此来判断本地文件是否过期,跟服务端无关
        /// </summary>
        public TimeSpan ExpiryTime { get; set; } = TimeSpan.FromHours(1);
    }
}
