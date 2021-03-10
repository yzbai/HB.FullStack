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
            X509Certificate2? certificate2 = CertificateUtil.GetBySubject(subject);

            if (certificate2 == null)
            {
                GlobalSettings.Logger.LogWarning2($"证书 {subject} 没有安装到服务器上，将试图寻找文件证书");

                certificate2 = CertificateUtil.GetByFileName(fullPath, password);

                if (certificate2 == null)
                {
                    GlobalSettings.Logger.LogCritical2(null, $"证书文件 {fullPath} 没有找到，将无法启动服务!");

#pragma warning disable CA2201 // Do not raise reserved exception types
                    throw new Exception($"证书没有找到，Subject:{subject} or File : {fullPath}");
#pragma warning restore CA2201 // Do not raise reserved exception types
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