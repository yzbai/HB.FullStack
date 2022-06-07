using Microsoft.Maui.Controls;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Maui.Base
{
    public abstract class BaseView : ContentView, IBaseContentView
    {
        public virtual void OnPageAppearing()
        {
            if (CustomerControls != null)
            {
                Parallel.ForEach(CustomerControls, v => v.OnPageAppearing());
            }
        }

        public virtual void OnPageDisappearing()
        {
            if (CustomerControls != null)
            {
                Parallel.ForEach(CustomerControls, v => v.OnPageDisappearing());
            }
        }

        //TODO: 使用SourceGeneration代替
        public IList<IBaseContentView>? CustomerControls { get; protected set; }
    }
}
