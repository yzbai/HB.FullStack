using System;
using System.Collections.Generic;
using System.Text;

namespace System.Security.Cryptography.X509Certificates
{
    public class CertificateUtil
    {
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

        public static X509Certificate2 GetBySubject(string subjectName, StoreLocation storeLocation, StoreName storeName)
        {
            using (X509Store store = new X509Store(storeName, storeLocation))
            {
                store.Open(OpenFlags.MaxAllowed);

                X509Certificate2Collection collection = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, subjectName, true);

                if (collection.Count != 1)
                {
                    return null;
                }

                return collection[0];
            }
        }

        public static X509Certificate2 GetByThumbprint(string thumbprint, StoreLocation storeLocation)
        {
            using (X509Store store = new X509Store(storeLocation))
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                X509Certificate2Collection collection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

                if (collection.Count != 1)
                {
                    return null;
                }

                return collection[0];
            }
        }
    }
}
