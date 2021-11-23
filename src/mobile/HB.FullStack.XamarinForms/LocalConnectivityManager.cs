using HB.FullStack.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace HB.FullStack.XamarinForms
{
    public class LocalConnectivityManager : ConnectivityManager
    {
        public override bool IsInternetConnected()
        {
            return Connectivity.NetworkAccess == NetworkAccess.Internet;
        }
    }
}
