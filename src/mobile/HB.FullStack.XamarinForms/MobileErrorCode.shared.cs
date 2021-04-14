namespace System
{
    public static class MobileErrorCodes
    {
        public static ErrorCode OutOfRange { get; set; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 0, nameof(OutOfRange), "");
        public static ErrorCode IdBarrierError { get; set; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 1, nameof(IdBarrierError), "");
        public static ErrorCode ResourceNotFound { get; set; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 2, nameof(ResourceNotFound), "");
        public static ErrorCode BizError { get; set; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 3, nameof(BizError), "");
        public static ErrorCode NotLogined { get; set; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 4, nameof(NotLogined), "");
        public static ErrorCode AliyunStsTokenError { get; set; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 5, nameof(AliyunStsTokenError), "");
        public static ErrorCode FileServiceError { get; set; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 6, nameof(FileServiceError), "");
        public static ErrorCode AliyunOssPutObjectError { get; set; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 7, nameof(AliyunOssPutObjectError), "");
        public static ErrorCode LocalFileCopyError { get; set; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 8, nameof(LocalFileCopyError), "");
        public static ErrorCode LocalFileSaveError { get; set; } = new ErrorCode(ErrorCodeStartIds.MOBILE + 9, nameof(LocalFileSaveError), "");

    }

    public static class MobileExceptions
    {
        internal static Exception LocalFileSaveError(string fullPath)
        {
            throw new NotImplementedException();
        }

        public static Exception AliyunOssPutObjectError(string cause)
        {
            throw new NotImplementedException();
        }

        public static Exception AliyunOssPutObjectError(string cause, Exception innerException)
        {
            throw new NotImplementedException();
        }

        public static Exception FileServiceError(string fileName, string directory, string cause, Exception innerException)
        {
            throw new NotImplementedException();
        }

        internal static Exception OutOfRange(int selectedIndex, string cause)
        {
            throw new NotImplementedException();
        }

        internal static Exception IdBarrierError(string cause)
        {
            throw new NotImplementedException();
        }

        internal static Exception ResourceNotFound(string resourceId)
        {
            throw new NotImplementedException();
        }
    }
}