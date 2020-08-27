using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HB.Framework.Client.Effects
{
    public class FocusEffect : RoutingEffect
    {
        public FocusEffect() : base($"HB.Framework.Client.Effects.{nameof(FocusEffect)}")
        {
        }
    }
}
