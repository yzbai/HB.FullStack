﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls
{
    public static class ShellExtensions
    {
        //不准确，比如无法处理D_FAULT_PageName12
        //public static string GetPageName(this ShellNavigationState state)
        //{
        //    List<char> charLst = new List<char>();

        //    char[] chars = state.Location.OriginalString.ToCharArray();

        //    bool trimflag = true;

        //    for (int i = chars.Length - 1; i >= 0; i--)
        //    {
        //        char c = chars[i];

        //        if (trimflag)
        //        {
        //            if (c == ' ' || c == '?' || c == '/')
        //            {
        //                continue;
        //            }
        //        }

        //        trimflag = false;

        //        if (c == '?')
        //        {
        //            charLst.Clear();
        //            continue;
        //        }
        //        else if (c == '/')
        //        {
        //            break;
        //        }
        //        else
        //        {
        //            charLst.Add(c);
        //        }
        //    }

        //    StringBuilder stringBuilder = new StringBuilder(charLst.Count);

        //    for(int i = charLst.Count -1; i>=0; i--)
        //    {
        //        stringBuilder.Append(charLst[i]);
        //    }

        //    return stringBuilder.ToString();
        //}
    }
}
