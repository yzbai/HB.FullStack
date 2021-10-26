using System;
using System.Diagnostics.CodeAnalysis;

namespace HB.FullStack.XamarinForms.Platforms
{
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
    public interface ILoggerImpl
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    {
        void Wtf(string message);



        void Error(string message);


        void Wtf(Exception exception);




        void Error(Exception exception);


        void Warn(string message);


        void Info(string message);


        void Debug(string message);


        void Trace(string message);

    }
}
