using System;
using Aliyun.OSS;

namespace AliyunOSSDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            string endpoint = "oss-cn-hangzhou.aliyuncs.com";
            string accessKeyId = "LTAIWPNZ6b5oYNA6";
            string accessKeySecret = "1ENmo2T9dLLxPYP20j3QFrLN72iXI7";

            IOss client = new OssClient(endpoint, accessKeyId, accessKeySecret);

            Console.WriteLine(Environment.CurrentDirectory);
            client.PutObject("ahabit", "test.txt", "test.txt");
        }
    }
}


//"ProductName": "Dysmsapi",
//        "RegionId": "cn-hangzhou",
//        "AccessUserName": "15190208956",
//        "AccessKeyId": "LTAI0xWtkSKjq3OU",
//        "AccessKeySecret": "OUbxdNB45fjMPW8w8hCo1ww2ygKlUT",
//        "Endpoint": "dysmsapi.aliyuncs.com"


//RoleArn: acs:ram::50186796:role/aliyunosstokengeneratorrole
//RoleSessionName: external-username
//DurationSeconds: 3600