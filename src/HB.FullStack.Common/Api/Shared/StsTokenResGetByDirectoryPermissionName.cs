using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public class StsTokenResGetByDirectoryPermissionName : GetRequest<StsTokenRes>
    {
        [Required]
        public string DirectoryPermissionName { get; set; } = null!;

        public string? RegexPlaceHolderValue { get; set; }

        public bool ReadOnly { get; set; }

        public StsTokenResGetByDirectoryPermissionName() { }

        public StsTokenResGetByDirectoryPermissionName(ApiRequestAuth auth, string directoryPermissionName, string? regexPlaceHolderValue, bool readOnly)
            : base(auth, "ByDirectoryPermissionName")
        {
            DirectoryPermissionName = directoryPermissionName;

            RegexPlaceHolderValue = regexPlaceHolderValue;
            ReadOnly = readOnly;
        }
    }
}