using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Security.Cryptography.X509Certificates
{
    public static class CertificateUtil
    {
        /// <summary>
        /// 在CurrentUser中和LocalMachine中寻找
        /// </summary>
        /// <param name="subjectName"></param>
        /// <returns></returns>
        public static X509Certificate2 GetBySubject(string subjectName)
        {
            X509Certificate2 cert = GetBySubject(subjectName, StoreLocation.CurrentUser) ?? GetBySubject(subjectName, StoreLocation.LocalMachine);

            if (cert == null)
            {
                LogHelper.GlobalLogger.LogCritical("证书找不到，SubjectName : {0}", subjectName);
            }

            return cert;
        }


        public static X509Certificate2 GetBySubject(string subjectName, StoreLocation storeLocation)
        {
            using (X509Store store = new X509Store(storeLocation))
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                X509Certificate2Collection collection = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, subjectName, true);

                if (collection.Count != 1)
                {
                    return null;
                }

                return collection[0];
            }
        }

        public static X509Certificate2 GetByThumbprint(string thumbprint)
        {
            X509Certificate2 cert = GetByThumbprint(thumbprint, StoreLocation.CurrentUser) ?? GetByThumbprint(thumbprint, StoreLocation.LocalMachine);

            if (cert == null)
            {
                LogHelper.GlobalLogger.LogCritical("证书找不到, thumbprint : {0}", thumbprint);
            }

            return cert;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thumbprint"></param>
        /// <param name="storeLocation"></param>
        /// <returns></returns>
        public static X509Certificate2 GetByThumbprint(string thumbprint, StoreLocation storeLocation)
        {
            using (X509Store store = new X509Store(storeLocation))
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                X509Certificate2Collection collection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, true);

                if (collection.Count != 1)
                {
                    return null;
                }

                return collection[0];
            }
        }
    }
}
