using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Client.UI.Platform
{
    public interface IStatusBar
    {
        bool IsShowing { get; }

        void Show();

        void Hide();
    }
}
