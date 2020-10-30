using System;
using System.Diagnostics.CodeAnalysis;

namespace HB.Framework.Client.Platforms
{
    public interface IPlatformLoggerImpl
    {
        void Wtf(string message);



        [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
        void Error(string message);


        void Wtf(Exception exception);




        [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
        void Error(Exception exception);


        void Warn(string message);


        void Info(string message);


        void Debug(string message);


        void Trace(string message);

    }
}
