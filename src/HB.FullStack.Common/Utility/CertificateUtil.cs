#nullable enable

using System.IO;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Extensions.Logging;

namespace System
{
    public static class CertificateUtil
    {
        public static X509Certificate2 GetCertificateFromSubjectOrFile(string? subject, string? fullPath, string? password)
        {
            X509Certificate2? certificate2 = CertificateUtil.GetByFileName(fullPath, password);

            if (certificate2 == null)
            {
                GlobalSettings.Logger?.LogCertNotInPackage(fullPath);

                certificate2 = GetBySubject(subject);

                if (certificate2 == null)
                {
                    throw CommonExceptions.CertNotFound(subject, fullPath);
                }
            }

            return certificate2;
        }

        /// <summary>
        /// 在CurrentUser中和LocalMachine中寻找
        /// </summary>
        /// <param name="subjectName"></param>
        /// <returns></returns>
        public static X509Certificate2? GetBySubject(string? subjectName)
        {
            if (subjectName.IsNullOrEmpty())
            {
                return null;
            }

            return GetBySubject(subjectName, StoreLocation.CurrentUser) ?? GetBySubject(subjectName, StoreLocation.LocalMachine);
        }

        public static X509Certificate2? GetBySubject(string subjectName, StoreLocation storeLocation)
        {
            using X509Store store = new X509Store(storeLocation);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            X509Certificate2Collection collection = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, subjectName, true);

            if (collection.Count != 1)
            {
                return null;
            }

            return collection[0];
        }

        public static X509Certificate2? GetByThumbprint(string thumbprint)
        {
            return GetByThumbprint(thumbprint, StoreLocation.CurrentUser) ?? GetByThumbprint(thumbprint, StoreLocation.LocalMachine);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="thumbprint"></param>
        /// <param name="storeLocation"></param>
        /// <returns></returns>
        public static X509Certificate2? GetByThumbprint(string thumbprint, StoreLocation storeLocation)
        {
            using X509Store store = new X509Store(storeLocation);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

            X509Certificate2Collection collection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, true);

            if (collection.Count != 1)
            {
                return null;
            }

            return collection[0];
        }

        public static X509Certificate2? GetByFileName(string? fullPath, string? certificateFilePassword)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                return null;
            }

            if (!File.Exists(fullPath))
            {
                return null;
            }

            return certificateFilePassword.IsNullOrEmpty() ? new X509Certificate2(fullPath) : new X509Certificate2(fullPath, certificateFilePassword);
        }

    }
}