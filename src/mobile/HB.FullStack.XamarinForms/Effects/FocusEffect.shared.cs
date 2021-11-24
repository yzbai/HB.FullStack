using HB.FullStack.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HB.FullStack.XamarinForms.Effects
{
    public class FocusEffect : RoutingEffect
    {
        public FocusEffect() : base($"{ClientConventions.EFFECTS_GROUP_NAME}.{nameof(FocusEffect)}")
        {
        }
    }
}
