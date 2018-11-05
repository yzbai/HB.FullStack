using System;

namespace HB.Component.Authorization.Abstractions
{
    public enum ClientType
    {
        None = 0,
        Android = 1,
        Iphone = 2,
        Web = 3,
        Postman = 4
    }

    public class ClientTypeChecker
    {
        public static ClientType Check(string clientType)
        {
            if (Enum.TryParse<ClientType>(clientType, out ClientType result))
            {
                if (Enum.IsDefined(typeof(ClientType), result))
                {
                    return result;
                }
            }

            return ClientType.None;
        }
    }
}
