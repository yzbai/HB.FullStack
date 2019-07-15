namespace HB.Framework.Http.SDK
{
    public interface IDeviceInfoProvider
    {
        string DeviceId { get; }

        string DeviceType { get; }

        string DeviceVersion { get; }

        string DeviceAddress { get; }
    }
}
