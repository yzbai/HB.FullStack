/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using Microsoft.Maui.Controls;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.FullStack.Client.MauiLib.Base
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