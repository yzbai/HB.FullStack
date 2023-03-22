using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common;

namespace System
{
    public class DeviceInfos : ValidatableObject
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Model { get; set; } = null!;

        [Required]
        public string OSVersion { get; set; } = null!;

        [Required]
        public string Platform { get; set; } = null!;

        [Required]
        public DeviceIdiom Idiom { get; set; } = DeviceIdiom.Unknown;

        [Required]
        public string Type { get; set; } = null!;
    }
}