using System.ComponentModel.DataAnnotations;

using HB.FullStack.Client.ApiClient;
using HB.FullStack.Common.Shared;


namespace HB.FullStack.Client.Files
{
    public class DirectoryTokenResGetByDirectoryPermissionNameRequest : ApiRequest //GetRequest<DirectoryTokenRes>
    {
        [Required]
        [RequestQuery]
        public string DirectoryPermissionName { get; set; } = null!;

        [RequestQuery]
        public string? PlaceHolderValue { get; set; }

        [RequestQuery]
        public bool ReadOnly { get; set; }

        public DirectoryTokenResGetByDirectoryPermissionNameRequest(string directoryPermissionName, string? regexPlaceHolderValue, bool readOnly)
            : base(nameof(DirectoryTokenRes), ApiMethod.Get, null, SharedNames.Conditions.ByDirectoryPermissionName)
        {
            DirectoryPermissionName = directoryPermissionName;
            PlaceHolderValue = regexPlaceHolderValue;
            ReadOnly = readOnly;
        }
    }
}