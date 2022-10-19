using Microsoft.Extensions.Logging;

namespace System
{
    //TODO: 也把其他的Exceptions类改造成这样
    //TODO: Source Generator
    public static partial class LoggerExtensions
    {
        private static readonly Action<ILogger, string?, Exception?> _logCerNotInPackage = LoggerMessage.Define<string?>(
            LogLevel.Warning,
            ErrorCodes.CertNotInPackage.ToEventId(),
            "证书 {FullPath} 没有打包，将试图在服务器中寻找");

        public static void LogCertNotInPackage(this ILogger logger, string? fullPath)
        {
            _logCerNotInPackage(logger, fullPath, null);
        }

        private static readonly Action<ILogger, string, Exception> _logIsFileSignatureMatchedError = LoggerMessage.Define<string>(
            LogLevel.Error,
            ErrorCodes.IsFileSignatureMatchedError.ToEventId(),
            "文件开头字节和FileExtension不一致，有可能篡改了文件格式. FileExtension = {FileExtension}");

        public static void LogIsFileSignatureMatchedError(this ILogger logger, string fileExtension, Exception ex)
        {
            _logIsFileSignatureMatchedError(logger, fileExtension, ex);
        }

        private static readonly Action<ILogger, string?, bool, bool, Exception?> _logSaveFileError = LoggerMessage.Define<string?, bool, bool>(
            LogLevel.Error,
            ErrorCodes.SaveFileError.ToEventId(),
            "文件保存出错，FullPath={FullPath}, CanOverride = {CanOverride}, CreateDirectoryIfNotExist={CreateDirectoryIfNotExist}");

        public static void LogSaveFileError(this ILogger logger, string fullPath, bool isOverride, bool createDirectoryIfNotExist, Exception ex)
        {
            _logSaveFileError(logger, fullPath, isOverride, createDirectoryIfNotExist, ex);
        }

        private static readonly Action<ILogger, string, Exception> _logReadFileError = LoggerMessage.Define<string>(
            LogLevel.Error,
            ErrorCodes.ReadFileError.ToEventId(),
            "文件读取出错，FullPath={FullPath}");

        public static void LogReadFileError(this ILogger logger, string fullPath, Exception ex)
        {
            _logReadFileError(logger, fullPath, ex);
        }

        private static readonly Action<ILogger, string?, int, string?, Uri?, Exception> _logHttpResponseDeSerializeJsonError = LoggerMessage.Define<string?, int, string?, Uri?>(
            LogLevel.Error,
            ErrorCodes.HttpResponseDeSerializeJsonError.ToEventId(),
            "解析HttpResponse的Json内容出错. Content={Content}, StatusCode={StatusCode}, ReasonPhrase={ReasonPhrase}, Uri={Uri}");

        public static void LogHttpResponseDeSerializeJsonError(this ILogger logger, string? content, int statusCode, string? reasonPhrase, Uri? uri, Exception ex)
        {
            _logHttpResponseDeSerializeJsonError(logger, content, statusCode, reasonPhrase, uri, ex);
        }

        private static readonly Action<ILogger, string?, Exception> _logSerializeLogError = LoggerMessage.Define<string?>(
            LogLevel.Error,
            ErrorCodes.SerializeLogError.ToEventId(),
            "序列化Json出错. TypeName={TypeName}");

        public static void LogSerializeLogError(this ILogger logger, string? typeName, Exception ex)
        {
            _logSerializeLogError(logger, typeName, ex);
        }

        private static readonly Action<ILogger, string?, Exception> _logUnSerializeLogError = LoggerMessage.Define<string?>(
            LogLevel.Error,
            ErrorCodes.UnSerializeLogError.ToEventId(),
            "反序列化Json出错. Json={Json}");

        public static void LogUnSerializeLogError(this ILogger logger, string? json, Exception ex)
        {
            _logUnSerializeLogError(logger, json, ex);
        }

        private static readonly Action<ILogger, string?, Exception> _logPerformValidateError = LoggerMessage.Define<string?>(
            LogLevel.Error,
            ErrorCodes.PerformValidateError.ToEventId(),
            "执行属性Validation出错。PropertyName={PropertyName}");

        public static void LogPerformValidateError(this ILogger logger, string? propertyName, Exception ex)
        {
            _logPerformValidateError(logger, propertyName, ex);
        }

        private static readonly Action<ILogger, string?, string?, Exception?> _logTryDeserializeWithCollectionCheckError = LoggerMessage.Define<string?, string?>(
            LogLevel.Error,
            ErrorCodes.TryFromJsonWithCollectionCheckError.ToEventId(),
            "解析json到可能是集合或者个体的时候出错。JsonString={JsonString}, TypeName={TypeName}");

        public static void LogTryDeserializeWithCollectionCheckError(this ILogger logger, string? jsonString, string? typeName, Exception? innerException)
        {
            _logTryDeserializeWithCollectionCheckError(logger, jsonString, typeName, innerException);
        }
    }
}
