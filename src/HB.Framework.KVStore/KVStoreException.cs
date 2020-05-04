using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace HB.Framework.KVStore
{
    public class KVStoreException : Exception
    {
        private IDictionary? _data;

        public KVStoreError Error { get; private set; }

        public int InnerNumber { get; private set; }

        public string? InnerState { get; private set; }

        public string? EntityName { get; private set; }

        public string? Operation { get; private set; }

        public KVStoreException(KVStoreError error, string entityName, string? message, Exception? innerException = null, [CallerMemberName]string operation = "") : base(message, innerException)
        {
            Error = error;
            Operation = operation;
            EntityName = entityName;

        }

        public KVStoreException(Exception innerException, string entityName, string message, [CallerMemberName]string operation = "") :base(message, innerException)
        {
            Operation = operation;
            EntityName = entityName;

            if (innerException is KVStoreException kVStoreException)
            {
                Error = kVStoreException.Error;
                
                InnerNumber = kVStoreException.InnerNumber;
                InnerState = kVStoreException.InnerState;
            }
            else
            {
                Error = KVStoreError.UnKown;
            }
        }

        public KVStoreException(int number, string state, string message, Exception? innerException = null) : base(message, innerException)
        {
            InnerNumber = number;
            InnerState = state;
            Error = KVStoreError.InnerError;
        }

        public override IDictionary Data {
            get {
                if (_data is null)
                {
                    _data = base.Data;
                }

                _data["KVStoreError"] = Error.ToString();
                _data["InnerNumber"] = InnerNumber;
                _data["InnerState"] = InnerState;
                _data["EntityName"] = EntityName;
                _data["Operation"] = Operation;

                return _data;
            }
        }

        public KVStoreException()
        {
        }

        public KVStoreException(string message) : base(message)
        {
        }

        public KVStoreException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
