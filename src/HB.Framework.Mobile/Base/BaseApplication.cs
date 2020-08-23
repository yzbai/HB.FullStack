using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace HB.Framework.Client.Base
{
    public class BaseApplication : Application
    {
        internal void UIExceptionHandler(Exception obj)
        {
            throw new NotImplementedException();
        }
    }
}
