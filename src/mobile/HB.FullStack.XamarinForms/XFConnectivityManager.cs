using HB.FullStack.Client.Network;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace HB.FullStack.XamarinForms
{
    public class XFConnectivityManager : ConnectivityManager
    {
        public XFConnectivityManager()
        {
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private bool _isDiposed;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if(!_isDiposed)
            {
                if(disposing)
                {
                    // managed
                }
                //unmanaged

                _isDiposed = true;
            }
        }

        public static bool IsInternetConnected()
        {
            return Connectivity.NetworkAccess == NetworkAccess.Internet;
        }
    }
}
