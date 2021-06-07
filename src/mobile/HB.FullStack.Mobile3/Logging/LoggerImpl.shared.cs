using System;
using System.Diagnostics.CodeAnalysis;

namespace HB.FullStack.Mobile.Platforms
{
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public interface ILoggerImpl
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
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
