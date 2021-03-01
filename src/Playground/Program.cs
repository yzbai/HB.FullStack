using System;
using System.IO;

using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Auth.Sts;
using Aliyun.Acs.Core.Profile;
using Aliyun.OSS;

namespace Playground
{

    class TestServer
    {
        static string StsEndpoint = "";
        static string AccessKey = "";
        static string AccessSecret = "";

        static string BucketName = "";

        static string Arn = "";

        static string RoleSessionName = "";

        public static readonly string RolePolicy = @"";

        private DefaultAcsClient _acsClient;

        public TestServer()
        {
            DefaultProfile.GetProfile().AddEndpoint("", "", "Sts", StsEndpoint);

            var profile = DefaultProfile.GetProfile("", AccessKey, AccessSecret);

            _acsClient = new DefaultAcsClient(profile);
        }


        internal Response Process(Request request)
        {
            //查看Client的权限, 记录访问等操作
            //......

            //请求Sts凭证并返回
            AssumeRoleRequest assumeRoleRequest = new AssumeRoleRequest
            {
                RoleArn = Arn,
                RoleSessionName = RoleSessionName,
                Policy = RolePolicy,
                DurationSeconds = 15 * 60
            };

            AssumeRoleResponse response = _acsClient.GetAcsResponse(assumeRoleRequest);

            return new Response(response.Credentials.AccessKeyId, response.Credentials.AccessKeySecret, response.Credentials.SecurityToken, response.Credentials.Expiration);

        }
    }

    class TestClient
    {
        static string OssEndpoint = "oss-cn-hangzhou.aliyuncs.com";
        static string BucketName = "";
        private string _fileName;

        public TestClient()
        {

        }

        internal Request RequestFile(string fileName)
        {
            _fileName = fileName;
            //先查看拥有的StsToken是否过期，如果过期，直接请求Oss；否则先请求服务器索取Sts；
            //如果请求Oss出现Sts过期问题，请求服务器新的Sts，然后请求Oss

            return new Request();
        }

        internal Stream GetFileFromOSS(string accessKeyId, string accessKeySecret, string securityToken)
        {
            OssClient client = new OssClient(OssEndpoint, accessKeyId, accessKeySecret, securityToken);

            OssObject rt = client.GetObject(BucketName, _fileName);
            //OssException
            return rt.Content;
        }
    }

    class Request
    {

    }

    class Response
    {
        public Response(string accessKeyId, string accessKeySecret, string securityToken, string expiration)
        {
            AccessKeyId = accessKeyId;
            AccessKeySecret = accessKeySecret;
            SecurityToken = securityToken;
            Expiration = expiration;
        }

        public string AccessKeyId { get; }
        public string AccessKeySecret { get; }
        public string SecurityToken { get; }
        public string Expiration { get; }
    }

    class Program
    {
        static string fileName = "Test.zip";

        static void Main(string[] args)
        {
            AliyunOssWithStsDemo();

        }

        private static void AliyunOssWithStsDemo()
        {
            TestServer server = new TestServer();
            TestClient client = new TestClient();

            Request request = client.RequestFile(fileName);

            Response response = server.Process(request);

            using Stream stream = client.GetFileFromOSS(response.AccessKeyId, response.AccessKeySecret, response.SecurityToken);
            using FileStream fileStream = new FileStream("LocalTest.zip", FileMode.Create);
            stream.CopyTo(fileStream);
            fileStream.Flush();
        }
    }
}
