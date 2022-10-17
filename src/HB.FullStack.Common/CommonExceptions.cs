using System.Runtime.CompilerServices;

using HB.FullStack.Common.Models;

namespace System
{
    public static partial class CommonExceptions
    {
        public static Exception AddtionalPropertyNeeded(string model, string property, [CallerMemberName] string? caller = null)
        {
            Exception ex = new CommonException(ErrorCodes.ChangedPackError, nameof(AddtionalPropertyNeeded), null, null);

            ex.Data["Model"] = model;
            ex.Data["Property"] = property;
            ex.Data["Caller"] = caller;

            return ex;
        }

        public static Exception CertNotFound(string? subject, string? fullPath)
        {
            return new CommonException(ErrorCodes.CertNotFound, nameof(CertNotFound), null, new { Subject = subject, FullPath = fullPath });
        }

        public static Exception EnvironmentVariableError(object? value, string cause)
        {
            return new CommonException(ErrorCodes.EnvironmentVariableError, cause, null, new { Value = value });
        }

        public static Exception UnkownEventSender(object? sender, [CallerMemberName] string? callerMethodName = null)
        {
            Exception ex = new CommonException(ErrorCodes.EventError, nameof(UnkownEventSender), null, null);

            ex.Data["SenderType"] = sender?.GetType().FullName;
            ex.Data["CallerMethodName"] = callerMethodName;

            return ex;
        }

        #region

        public static Exception ServerUnkownError(string responseString)
        {
            return new CommonException(ErrorCodes.ServerUnKownError, "Server返回了其他格式的错误表示，赶紧处理", null, new { Response = responseString });
        }

        public static Exception ApiClientInnerError(string cause, Exception? innerEx, object? context)
        {
            return new CommonException(ErrorCodes.ApiClientInnerError, cause, innerEx, context);
        }

        public static Exception ServerReturnError(ErrorCode errorCode)
        {
            return new CommonException(errorCode, "Server认为请求无法返回正确", null, null);
        }

        public static Exception ApiModelError(string cause, Exception? innerEx, object? context)
        {
            return new CommonException(ErrorCodes.ApiModelError, cause, innerEx, context);
        }

        public static Exception ApiAuthenticationError(string cause, Exception? innerEx, object? context)
        {
            return new CommonException(ErrorCodes.ApiAuthenticationError, cause, innerEx, context);
        }

        public static Exception ApiResourceError(string cause, Exception? innerEx, object? context)
        {
            return new CommonException(ErrorCodes.ApiResourceError, cause, innerEx, context);
        }

        public static Exception ServerNullReturn(string parameter)
        {
            return new CommonException(ErrorCodes.ServerNullReturn, "Server端返回NULL", null, new { Parameter = parameter });
        }

        /// <summary>
        /// 不能为非Model类型的类获取ModelDef
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static Exception CannotGetModelDefForNonModel(Type type)
        {
            CommonException ex = new CommonException(ErrorCodes.ModelDefError, nameof(CannotGetModelDefForNonModel), null, null);
            ex.Data["Type"] = type.FullName;

            return ex;
        }

        internal static Exception LackModelDefProvider(Type type, ModelKind modelKind)
        {
            CommonException ex = new CommonException(ErrorCodes.ModelDefError, nameof(LackModelDefProvider), null, null);

            ex.Data["Type"] = type.FullName;
            ex.Data["ModelKind"] = modelKind;

            return ex;
        }

        internal static Exception ModelDefProviderDidNotProvideModelDef(Type type, ModelKind modelKind)
        {
            CommonException ex = new CommonException(ErrorCodes.ModelDefError, nameof(ModelDefProviderDidNotProvideModelDef), null, null);

            ex.Data["Type"] = type.FullName;
            ex.Data["ModelKind"] = modelKind;

            return ex;
        }

        #endregion

    }
}
