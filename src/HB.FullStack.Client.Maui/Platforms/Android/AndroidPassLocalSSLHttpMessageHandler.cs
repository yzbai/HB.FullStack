using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Javax.Net.Ssl;

using Xamarin.Android.Net;

namespace HB.FullStack.Client.Maui.Platforms.Android
{
    public class AndroidPassLocalSSLHttpMessageHandler : AndroidMessageHandler
    {
        public AndroidPassLocalSSLHttpMessageHandler()
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert!.Issuer.Equals("CN=localhost", GlobalSettings.Comparison))
                    return true;
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
        }

        protected override IHostnameVerifier? GetSSLHostnameVerifier(HttpsURLConnection connection)
        {
            return new AndroidPassLocalSSLHostnameVerifier();
            //return base.GetSSLHostnameVerifier(connection);
        }
    }

    public class AndroidPassLocalSSLHostnameVerifier : Java.Lang.Object, Javax.Net.Ssl.IHostnameVerifier
    {
        public bool Verify(string? hostname, ISSLSession? session)
        {
            if(hostname == "10.0.2.2" && session?.PeerPrincipal?.Name == "CN=localhost")
            {
                return true;
            }

            return Javax.Net.Ssl.HttpsURLConnection.DefaultHostnameVerifier!.Verify(hostname, session);
        }
    }
}
