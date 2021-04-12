using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Api
{
    public static class ApiErrorCodes
    {
        //ApiUnkown

        public static ErrorCode NoAuthority { get; set; } = new ErrorCode(8000, nameof(NoAuthority), "");
        public static ErrorCode AccessTokenExpired { get; set; } = new ErrorCode(8001, nameof(AccessTokenExpired), "");
        public static ErrorCode ModelValidationError { get; set; } = new ErrorCode(8002, nameof(ModelValidationError), "");
        public static ErrorCode ApiNotAvailable { get; set; } = new ErrorCode(8003, nameof(ApiNotAvailable), "");
        public static ErrorCode ApiErrorUnkownFormat { get; set; } = new ErrorCode(8003, nameof(ApiErrorUnkownFormat), "");
        public static ErrorCode NotApiResourceEntity { get; set; } = new ErrorCode(8004, nameof(NotApiResourceEntity), "");
        public static ErrorCode ApiSmsCodeInvalid { get; set; } = new ErrorCode(8005, nameof(ApiSmsCodeInvalid), "");
        public static ErrorCode SmsServiceError { get; set; } = new ErrorCode(8006, nameof(SmsServiceError), "");
        public static ErrorCode PublicResourceTokenNeeded { get; set; } = new ErrorCode(8007, nameof(PublicResourceTokenNeeded), "");
        public static ErrorCode PublicResourceTokenError { get; set; } = new ErrorCode(8008, nameof(PublicResourceTokenError), "");
        public static ErrorCode ApiUploadEmptyFile { get; set; } = new ErrorCode(8009, nameof(ApiUploadEmptyFile), "");
        public static ErrorCode ApiUploadOverSize { get; set; } = new ErrorCode(8010, nameof(ApiUploadOverSize), "");
        public static ErrorCode ApiUploadWrongType { get; set; } = new ErrorCode(8011, nameof(ApiUploadWrongType), "");
        public static ErrorCode HttpsRequired { get; set; } = new ErrorCode(8012, nameof(HttpsRequired), "");
        public static ErrorCode FromExceptionController { get; set; } = new ErrorCode(8013, nameof(FromExceptionController), "");
        public static ErrorCode ApiCapthaError { get; set; } = new ErrorCode(8014, nameof(ApiCapthaError), "");
        public static ErrorCode ApiUploadFailed { get; set; } = new ErrorCode(8015, nameof(ApiUploadFailed), "");
        public static ErrorCode ServerError { get; set; } = new ErrorCode(8016, nameof(ServerError), "");
        public static ErrorCode ClientError { get; set; } = new ErrorCode(8017, nameof(ClientError), "");
        public static ErrorCode NullReturn { get; set; } = new ErrorCode(8018, nameof(NullReturn), "");
        public static ErrorCode Timeout { get; set; } = new ErrorCode(8019, nameof(Timeout), "");
        public static ErrorCode RequestCanceled { get; set; } = new ErrorCode(8020, nameof(RequestCanceled), "");
        public static ErrorCode AliyunStsTokenReturnNull { get; set; } = new ErrorCode(8021, nameof(AliyunStsTokenReturnNull), "");
        public static ErrorCode AliyunOssPutObjectError { get; set; } = new ErrorCode(8022, nameof(AliyunOssPutObjectError), "");
        public static ErrorCode TokenRefreshError { get; set; } = new ErrorCode(8023, nameof(TokenRefreshError), "");
        public static ErrorCode UserActivityFilterError { get; set; } = new ErrorCode(8024, nameof(UserActivityFilterError), "");

    }

    public static class ApiExceptions
    {
        public static Exception ApiNotAvailable(ApiRequest request, Exception innerException)
        {
            throw new NotImplementedException();
            //ApiException exception = new ApiException(ApiErrorCodes.ApiNotAvailable, innerException);

            //return exception;
        }

        public static Exception RequestCanceled(ApiRequest request, Exception innerException)
        {
            throw new NotImplementedException();
        }

        public static Exception ClientTimeout(ApiRequest request, Exception innerException)
        {
            throw new NotImplementedException();
        }

        public static Exception ClientUnkownError(ApiRequest request, Exception innerException)
        {
            throw new NotImplementedException();
        }

        public static Exception ServerUnkownError(HttpResponseMessage response)
        {
            throw new NotImplementedException();
        }

        public static Exception ClientError(string cause, Exception innerException)
        {
            throw new NotImplementedException();
        }

        public static Exception ServerReturnError(ErrorCode errorCode)
        {
            throw new NotImplementedException();
        }

        public static Exception NotApiResourceEntity(string type)
        {
            throw new NotImplementedException();
        }

        public static Exception AliyunOssPutObjectError()
        {
            throw new NotImplementedException();
        }

        public static Exception ApiUploadEmptyFile()
        {
            throw new NotImplementedException();
        }

        public static Exception ApiUploadOverSize()
        {
            throw new NotImplementedException();
        }

        public static Exception ApiUploadWrongType()
        {
            throw new NotImplementedException();
        }

        public static Exception ServerUnkownError(string fileName, Exception innerException)
        {
            throw new NotImplementedException();
        }

        public static Exception ModelValidationError(string cause)
        {
            throw new NotImplementedException();
        }

        public static Exception NoAuthority()
        {
            throw new NotImplementedException();
        }

        public static Exception TokenRefreshError(string cause)
        {
            throw new NotImplementedException();
        }

        public static Exception NoInternet(string cause)
        {
            throw new NotImplementedException();
        }

        public static Exception ServerNullReturn(string parameter)
        {
            throw new NotImplementedException();
        }

        public static Exception AliyunStsTokenReturnNull()
        {
            throw new NotImplementedException();
        }
    }
}
