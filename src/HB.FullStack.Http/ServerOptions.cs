using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HB.FullStack.Server
{
    public class ServerOptions : IOptions<ServerOptions>
    {
        public ServerOptions Value
        {
            get
            {
                return this;
            }
        }

        public int PublicResourceTokenExpireSeconds { get; set; } = 60;

        public FileSettings FileSettings { get; set; } = new FileSettings();

    }

    public class FileSettings
    {
        public string PublicPath { get; set; } = @"C:\MyColorfulTime\Public";

        public string ProtectedPath { get; set; } = @"C:\MyColorfulTime\Protected";

        public string PrivatePath { get; set; } = @"C:\MyColorfulTime\Private";

        public string AvatarPath { get; set; } = @"C:\MyColorfulTime\Protected\Avatars";

        public int AvatarMaxSize { get; set; } = 2097152;
    }
}
