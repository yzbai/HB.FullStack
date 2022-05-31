using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using HB.FullStack.Client.File;
using HB.FullStack.Client.Network;

using Microsoft;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace HB.FullStack.Client.Maui.Base
{
    public abstract class BaseApplication : Application
    {
        public new static BaseApplication Current => (BaseApplication?)Application.Current!;

        public abstract void OnOfflineDataUsed();
        
    }
}