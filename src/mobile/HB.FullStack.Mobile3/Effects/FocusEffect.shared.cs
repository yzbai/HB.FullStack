using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HB.FullStack.Mobile.Effects
{
    public class FocusEffect : RoutingEffect
    {
        public FocusEffect() : base($"{Conventions.EffectsGroupName}.{nameof(FocusEffect)}")
        {
        }
    }
}
