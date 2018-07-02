using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.EventBus
{
    public enum EventBusPublishResultStatus
    {
        Succeeded,
        Failed
    }

    public class PublishResult
    {
        public Exception Exception { get; private set; }

        public EventBusPublishResultStatus Status { get; set; }

        public static PublishResult Fail(Exception ex)
        {
            return new PublishResult() {
                Exception = ex ?? null,
                Status = EventBusPublishResultStatus.Failed
            };
        }

        public static PublishResult Fail(string message)
        {
            return Fail(new Exception(message));
        }

        public static PublishResult Succeeded()
        {
            return new PublishResult { Status = EventBusPublishResultStatus.Succeeded };
        }

        //public static readonly PublishResult Succeeded = new PublishResult() { Status = EventBusPublishResultStatus.Succeeded };
        //public static readonly PublishResult Failed = new PublishResult() { Status = EventBusPublishResultStatus.Failed };

        //public static bool operator ==(PublishResult left, PublishResult right)
        //{
        //    if (System.Object.ReferenceEquals(left, right))
        //    {
        //        return true;
        //    }

        //    if (((object)left == null) || ((object)right == null))
        //    {
        //        return false;
        //    }

        //    return left.Status == right.Status;
        //}

        //public static bool operator !=(PublishResult left, PublishResult right)
        //{
        //    return !(left == right);
        //}

        //public override bool Equals(object obj)
        //{
        //    if (obj == null)
        //    {

        //    }

        //    PublishResult result = obj as PublishResult;

        //    return this == result;
        //}

        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}
    }
}
