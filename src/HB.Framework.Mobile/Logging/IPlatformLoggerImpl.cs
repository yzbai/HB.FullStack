using System;

namespace HB.Framework.Client.Logging
{
    public interface IPlatformLoggerImpl
    {
        void Wtf(string message);



        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
        void Error(string message);


        void Wtf(Exception exception);




        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "<Pending>")]
        void Error(Exception exception);


        void Warn(string message);


        void Info(string message);


        void Debug(string message);


        void Trace(string message);

    }
}
