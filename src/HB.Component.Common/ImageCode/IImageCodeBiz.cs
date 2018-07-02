using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Compnent.Common.ImageCode
{
    public interface IImageCodeBiz
    {
        ImageCodeResult Create(int width, int height);
    }
}
