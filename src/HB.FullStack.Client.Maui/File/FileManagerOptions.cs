﻿using Microsoft.Extensions.Options;

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

        public TimeSpan AliyunStsTokenExpiryTime { get; set; } = TimeSpan.FromHours(1);

        #endregion

        #region Directory

        public TimeSpan DefaultFileExpiryTime { get; set; } = TimeSpan.FromHours(1);

        public IList<DirectoryDescription> Directories { get; set; } = new List<DirectoryDescription>();

        //public string PublicDirectory { get; set; } = "public";

        //public string PublicUploadDirectory { get; set; } = "public_upload";
        //public string CustomerAvatarDirectory { get; set; } = "customer/avatar";
        //public string CurrentUserTempDirectory { get; set; } = "customer/temp";
        //public string CustomerVipDirectory { get; set; } = "customer_vip";
        //public string ThemeDirectory { get; set; } = "theme";
        #endregion

        public FileManagerOptions Value => this;

    }

    public class DirectoryDescription
    {
        public string DirectoryName { get; set; } = null!;

        public string DirectoryPath { get; set; } = null!;

        /// <summary>
        /// path中可以出现UsrePlaceHolder，后期进行替换
        /// </summary>
        public string UserPlaceHolder { get; set; } = "{User}";

        public bool NeedUserPlace { get; set; } = true;

        public TimeSpan ExpiryTime { get; set; } = TimeSpan.FromHours(1);


    }
}
