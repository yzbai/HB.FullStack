using System.Collections.Generic;

namespace HB.FullStack.Client
{
    public interface IBaseContentView
    {
        //bool IsAppearing { get; }

        void OnPageAppearing();

        void OnPageDisappearing();

        IList<IBaseContentView>? CustomerControls { get; }
    }
}
