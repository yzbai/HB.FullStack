using System.ComponentModel.DataAnnotations;

using HB.FullStack.Client.ApiClient;
using HB.FullStack.Common.Shared;
using HB.FullStack.Common.Shared.Resources;

namespace HB.FullStack.Client.Components.Sts
{
    public class StsTokenResGetByDirectoryPermissionNameRequest : ApiRequest //GetRequest<StsTokenRes>
    {
        [Required]
        [RequestQuery]
        public string DirectoryPermissionName { get; set; } = null!;

        [RequestQuery]
        public string? RegexPlaceHolderValue { get; set; }

        [RequestQuery]
        public bool ReadOnly { get; set; }

        public StsTokenResGetByDirectoryPermissionNameRequest(string directoryPermissionName, string? regexPlaceHolderValue, bool readOnly)
            : base(nameof(StsTokenRes), ApiMethod.Get, null, SharedNames.Conditions.ByDirectoryPermissionName)
        {
            DirectoryPermissionName = directoryPermissionName;
            RegexPlaceHolderValue = regexPlaceHolderValue;
            ReadOnly = readOnly;
        }
    }
}