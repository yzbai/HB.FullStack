using System.Collections.Generic;

using HB.FullStack.Common.Files;

using Microsoft.Extensions.Options;

namespace HB.FullStack.Server.WebLib.Services
{
    public class DirectoryOptions : IOptions<DirectoryOptions>
    {
        public DirectoryOptions Value => this;


        public string AliyunOssEndpoint { get; set; } = "oss-cn-hangzhou.aliyuncs.com";

        public string AliyunOssBucketName { get; set; } = null!;

        public IList<DirectoryDescription> DirectoryDescriptions { get; set; } = new List<DirectoryDescription>();

        public IList<DirectoryPermission> DirectoryPermissions { get; set; } = new List<DirectoryPermission>();
    }
}
