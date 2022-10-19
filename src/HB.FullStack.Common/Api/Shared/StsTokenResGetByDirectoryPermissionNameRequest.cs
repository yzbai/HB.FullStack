using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
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
            : base(nameof(StsTokenRes), ApiMethod.Get, null, "ByDirectoryPermissionName")
        {
            DirectoryPermissionName = directoryPermissionName;
            RegexPlaceHolderValue = regexPlaceHolderValue;
            ReadOnly = readOnly;
        }
    }
}