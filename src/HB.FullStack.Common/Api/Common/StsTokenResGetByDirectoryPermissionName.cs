using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    public class StsTokenResGetByDirectoryPermissionName : GetRequest<StsTokenRes>
    {
        [Required]
        public string DirectoryPermissionName { get; set; } = null!;

        public string? RegexPlaceHolderValue { get; set; }

        public bool ReadOnly { get; set; }

        public StsTokenResGetByDirectoryPermissionName() { }

        public StsTokenResGetByDirectoryPermissionName(ApiRequestAuth auth, string directoryPermissionName, string? regexPlaceHolderValue, bool readOnly)
            : base(nameof(StsTokenRes), auth, "ByDirectoryPermissionName")
        {
            DirectoryPermissionName = directoryPermissionName;

            RegexPlaceHolderValue = regexPlaceHolderValue;
            ReadOnly = readOnly;
        }
    }
}