namespace System
{
    public static class MobileErrorCodes
    {
        public static ErrorCode ImageOptionsOutOfRange { get; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 0, nameof(ImageOptionsOutOfRange), "");
        public static ErrorCode IdBarrierError { get; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 1, nameof(IdBarrierError), "");
        public static ErrorCode ResourceNotFound { get; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 2, nameof(ResourceNotFound), "");
        public static ErrorCode BizError { get; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 3, nameof(BizError), "");
        public static ErrorCode NotLogined { get; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 4, nameof(NotLogined), "");
        public static ErrorCode AliyunStsTokenError { get; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 5, nameof(AliyunStsTokenError), "");
        public static ErrorCode FileServiceError { get; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 6, nameof(FileServiceError), "");
        public static ErrorCode AliyunOssPutObjectError { get; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 7, nameof(AliyunOssPutObjectError), "");
        public static ErrorCode LocalFileCopyError { get; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 8, nameof(LocalFileCopyError), "");
        public static ErrorCode LocalFileSaveError { get; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 9, nameof(LocalFileSaveError), "");
        public static ErrorCode AliyunStsTokenOverTime { get; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 10, nameof(AliyunStsTokenOverTime), "");
        public static ErrorCode SmsCodeValidateError { get; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 11, nameof(SmsCodeValidateError), "");
        public static ErrorCode NoInternet { get; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 12, nameof(NoInternet), "");
        public static ErrorCode UploadError { get; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 13, nameof(UploadError), "");
    }

    public static class MobileExceptions
    {
        internal static Exception LocalFileSaveError(string fullPath)
        {
            MobileException ex = new MobileException(MobileErrorCodes.LocalFileSaveError);

            ex.Data["FullPath"] = fullPath;

            return ex;
        }

        internal static Exception NoInternet(string cause)
        {
            MobileException ex = new MobileException(MobileErrorCodes.NoInternet);

            ex.Data["Cause"] = cause;

            return ex;
        }

        public static Exception AliyunOssPutObjectError(string cause, Exception? innerException)
        {
            MobileException ex = new MobileException(MobileErrorCodes.AliyunOssPutObjectError, innerException);

            ex.Data["Cause"] = cause;

            return ex;
        }

        public static Exception FileServiceError(string fileName, string directory, string cause, Exception innerException)
        {
            MobileException ex = new MobileException(MobileErrorCodes.FileServiceError, innerException);

            ex.Data["FileName"] = fileName;
            ex.Data["Directory"] = directory;
            ex.Data["Cause"] = cause;

            return ex;
        }

        internal static Exception ImageOptionsOutOfRange(int selectedIndex, string cause)
        {
            MobileException ex = new MobileException(MobileErrorCodes.ImageOptionsOutOfRange);

            ex.Data["SelectedIndex"] = selectedIndex;
            ex.Data["Cause"] = cause;

            return ex;
        }

        internal static Exception IdBarrierError(string cause)
        {
            MobileException ex = new MobileException(MobileErrorCodes.IdBarrierError);

            ex.Data["Cause"] = cause;

            return ex;
        }

        internal static Exception ResourceNotFound(string resourceId)
        {
            MobileException ex = new MobileException(MobileErrorCodes.ResourceNotFound);

            ex.Data["ResourceId"] = resourceId;

            return ex;
        }

        public static Exception AliyunStsTokenOverTime(string casuse, string requestDirectory, bool needWrite)
        {
            MobileException ex = new MobileException(MobileErrorCodes.AliyunStsTokenOverTime);

            ex.Data["Cause"] = casuse;
            ex.Data["RequestDirectory"] = requestDirectory;
            ex.Data["NeedWrite"] = needWrite.ToString();

            return ex;
        }

        public static Exception NotLogined()
        {
            MobileException ex = new MobileException(MobileErrorCodes.NotLogined);

            return ex;
        }

        public static Exception SmsCodeValidateError(string mobile)
        {
            MobileException ex = new MobileException(MobileErrorCodes.SmsCodeValidateError);

            ex.Data["Mobile"] = mobile;

            return ex;
        }

        internal static Exception UploadError(string cause)
        {
            MobileException ex = new MobileException(MobileErrorCodes.UploadError);

            ex.Data["Cause"] = cause;

            return ex;
        }
    }
}