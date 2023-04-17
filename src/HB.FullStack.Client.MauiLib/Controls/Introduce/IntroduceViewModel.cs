using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Files;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Client.MauiLib.Controls
{
    public class IntroduceViewModel : BaseViewModel
    {
        public IntroduceViewModel(ILogger logger, IPreferenceProvider preferenceProvider, IFileManager fileManager) : base(logger, preferenceProvider, fileManager)
        {
        }

        public override Task OnPageAppearingAsync()
        {
            throw new NotImplementedException();
        }

        public override Task OnPageDisappearingAsync()
        {
            throw new NotImplementedException();
        }
    }
}
