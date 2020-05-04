using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace HB.Framework.KVStore
{
    public class KVStoreException : FrameworkException
    {
        private IDictionary? _data;

        public override IDictionary Data
        {
            get
            {
                if (_data is null)
                {
                    _data = base.Data;
                }

                _data["KVStoreError"] = Error.ToString();
                _data["EntityName"] = EntityName;
                _data["Operation"] = Operation;

                return _data;
            }
        }

        public override FrameworkExceptionType ExceptionType { get => FrameworkExceptionType.KVStore; }

        public KVStoreError Error { get; private set; }

        public string? EntityName { get; private set; }

        public string? Operation { get; private set; }

        public KVStoreException(KVStoreError error, string entityName, string? message, Exception? innerException = null, [CallerMemberName] string operation = "") : this(message, innerException)
        {
            Error = error;
            Operation = operation;
            EntityName = entityName;

        }

        public KVStoreException(Exception innerException, string entityName, string message, [CallerMemberName] string operation = "") : this(message, innerException)
        {
            Operation = operation;
            EntityName = entityName;
        }

        public KVStoreException()
        {
        }

        public KVStoreException(string? message) : base(message)
        {
        }

        public KVStoreException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
