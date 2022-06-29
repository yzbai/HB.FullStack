﻿using HB.FullStack.Common.Files;

using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Maui.File
{
    public class FileManagerOptions : IOptions<FileManagerOptions>
    {
        //[ApiResource(Endpoints.MAIN_API, Versions.V1, ApiAuthType.Jwt, Names.AliyunStsTokens)]

        #region Aliyun

        public string AliyunOssEndpoint { get; set; } = "oss-cn-hangzhou.aliyuncs.com";

        public string AliyunOssBucketName { get; set; } = null!;

        public string AliyunStsTokenRequestUrl { get; set; } = null!;

        #endregion

        #region Directory

        public IList<DirectoryDescription> DirectoryDescriptions { get; set; } = new List<DirectoryDescription>();

        public IList<DirectoryPermission> DirectoryPermissions { get; set; } = new List<DirectoryPermission>(); 

        #endregion

        public FileManagerOptions Value => this;

    }
}
