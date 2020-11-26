using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HB.FullStack.Client.Effects
{
    public class FocusEffect : RoutingEffect
    {
        public FocusEffect() : base($"{ClientGlobal.EffectsGroupName}.{nameof(FocusEffect)}")
        {
        }
    }
}
