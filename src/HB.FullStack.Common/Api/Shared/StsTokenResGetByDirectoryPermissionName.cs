using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public class StsTokenResGetByDirectoryPermissionName : ApiRequest //GetRequest<StsTokenRes>
    {
        [Required]
        public string DirectoryPermissionName { get; set; } = null!;

        public string? RegexPlaceHolderValue { get; set; }

        public bool ReadOnly { get; set; }

        public StsTokenResGetByDirectoryPermissionName() { }

        public StsTokenResGetByDirectoryPermissionName(string directoryPermissionName, string? regexPlaceHolderValue, bool readOnly)
            : base(nameof(StsTokenRes), ApiMethodName.Get, null, "ByDirectoryPermissionName")
        {
            DirectoryPermissionName = directoryPermissionName;

            RegexPlaceHolderValue = regexPlaceHolderValue;
            ReadOnly = readOnly;
        }
    }
}