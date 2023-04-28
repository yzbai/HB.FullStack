using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common;

namespace HB.FullStack.Common.Shared
{
    public enum DeviceIdiom
    {
        Unknown,
        Phone,
        Tablet,
        Desktop,
        TV,
        Watch,
        Web
    }

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