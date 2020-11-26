using System;
using System.Collections.Generic;
using System.Text;
using HB.FullStack.Client.Base;
using Xamarin.Forms;
using Xamarin.Forms.Shapes;

namespace HB.FullStack.Client.Behaviors
{
    public class CircleClipBehavior : BaseBehavior<View>
    {
        protected override void OnAttachedTo(View bindable)
        {
            base.OnAttachedTo(bindable);

            bindable.SizeChanged += Bindable_SizeChanged;
        }

        private void Bindable_SizeChanged(object sender, EventArgs e)
        {
            if (sender is View view)
            {
                EllipseGeometry ellipseGeometry = new EllipseGeometry
                {
                    Center = new Point(view.Width / 2.0, view.Height / 2.0),
                    RadiusX = view.Width / 2.0,
                    RadiusY = view.Height / 2.0
                };

                view.Clip = ellipseGeometry;
            };
        }

        protected override void OnDetachingFrom(View bindable)
        {
            base.OnDetachingFrom(bindable);

            bindable.SizeChanged -= Bindable_SizeChanged;
        }
    }
}
