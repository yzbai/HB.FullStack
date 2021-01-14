using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.Mobile.Platforms
{
    public interface IStatusBarHelper
    {
        bool IsShowing { get; }

        void Show();

        void Hide();
    }
}
