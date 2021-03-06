﻿using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace HB.FullStack.XamarinForms
{
    public class ColorItem
    {
        public Color Color { get; set; }
        public string Name { get; set; } = null!;
    }

    public static class ColorUtil
    {
        private static readonly IList<ColorItem> _systemColorItems = new List<ColorItem>
        {
            new ColorItem() { Color = Color.Accent, Name = "Accent" },
            new ColorItem() { Color = Color.AliceBlue, Name = "AliceBlue" },
            new ColorItem() { Color = Color.MintCream, Name = "MintCream" },
            new ColorItem() { Color = Color.MistyRose, Name = "MistyRose" },
            new ColorItem() { Color = Color.Moccasin, Name = "Moccasin" },
            new ColorItem() { Color = Color.NavajoWhite, Name = "NavajoWhite" },
            new ColorItem() { Color = Color.Navy, Name = "Navy" },
            new ColorItem() { Color = Color.OldLace, Name = "OldLace" },
            new ColorItem() { Color = Color.MidnightBlue, Name = "MidnightBlue" },
            new ColorItem() { Color = Color.Olive, Name = "Olive" },
            new ColorItem() { Color = Color.Orange, Name = "Orange" },
            new ColorItem() { Color = Color.OrangeRed, Name = "OrangeRed" },
            new ColorItem() { Color = Color.Orchid, Name = "Orchid" },
            new ColorItem() { Color = Color.PaleGoldenrod, Name = "PaleGoldenrod" },
            new ColorItem() { Color = Color.PaleGreen, Name = "PaleGreen" },
            new ColorItem() { Color = Color.PaleTurquoise, Name = "PaleTurquoise" },
            new ColorItem() { Color = Color.OliveDrab, Name = "OliveDrab" },
            new ColorItem() { Color = Color.PaleVioletRed, Name = "PaleVioletRed" },
            new ColorItem() { Color = Color.MediumVioletRed, Name = "MediumVioletRed" },
            new ColorItem() { Color = Color.MediumSpringGreen, Name = "MediumSpringGreen" },
            new ColorItem() { Color = Color.LightSkyBlue, Name = "LightSkyBlue" },
            new ColorItem() { Color = Color.LightSlateGray, Name = "LightSlateGray" },
            new ColorItem() { Color = Color.LightSteelBlue, Name = "LightSteelBlue" },
            new ColorItem() { Color = Color.LightYellow, Name = "LightYellow" },
            new ColorItem() { Color = Color.Lime, Name = "Lime" },
            new ColorItem() { Color = Color.LimeGreen, Name = "LimeGreen" },
            new ColorItem() { Color = Color.MediumTurquoise, Name = "MediumTurquoise" },
            new ColorItem() { Color = Color.Linen, Name = "Linen" },
            new ColorItem() { Color = Color.Maroon, Name = "Maroon" },
            new ColorItem() { Color = Color.MediumAquamarine, Name = "MediumAquamarine" },
            new ColorItem() { Color = Color.MediumBlue, Name = "MediumBlue" },
            new ColorItem() { Color = Color.MediumOrchid, Name = "MediumOrchid" },
            new ColorItem() { Color = Color.MediumSeaGreen, Name = "MediumSeaGreen" },
            new ColorItem() { Color = Color.MediumSlateBlue, Name = "MediumSlateBlue" },
            new ColorItem() { Color = Color.Magenta, Name = "Magenta" },
            new ColorItem() { Color = Color.LightSeaGreen, Name = "LightSeaGreen" },
            new ColorItem() { Color = Color.PapayaWhip, Name = "PapayaWhip" },
            new ColorItem() { Color = Color.Peru, Name = "Peru" },
            new ColorItem() { Color = Color.SpringGreen, Name = "SpringGreen" },
            new ColorItem() { Color = Color.SteelBlue, Name = "SteelBlue" },
            new ColorItem() { Color = Color.Tan, Name = "Tan" },
            new ColorItem() { Color = Color.Teal, Name = "Teal" },
            new ColorItem() { Color = Color.Thistle, Name = "Thistle" },
            new ColorItem() { Color = Color.Tomato, Name = "Tomato" },
            new ColorItem() { Color = Color.Snow, Name = "Snow" },
            new ColorItem() { Color = Color.Transparent, Name = "Transparent" },
            new ColorItem() { Color = Color.Violet, Name = "Violet" },
            new ColorItem() { Color = Color.Wheat, Name = "Wheat" },
            new ColorItem() { Color = Color.White, Name = "White" },
            new ColorItem() { Color = Color.WhiteSmoke, Name = "WhiteSmoke" },
            new ColorItem() { Color = Color.Yellow, Name = "Yellow" },
            new ColorItem() { Color = Color.YellowGreen, Name = "YellowGreen" },
            new ColorItem() { Color = Color.Turquoise, Name = "Turquoise" },
            new ColorItem() { Color = Color.PeachPuff, Name = "PeachPuff" },
            new ColorItem() { Color = Color.SlateGray, Name = "SlateGray" },
            new ColorItem() { Color = Color.SkyBlue, Name = "SkyBlue" },
            new ColorItem() { Color = Color.Pink, Name = "Pink" },
            new ColorItem() { Color = Color.Plum, Name = "Plum" },
            new ColorItem() { Color = Color.PowderBlue, Name = "PowderBlue" },
            new ColorItem() { Color = Color.Purple, Name = "Purple" },
            new ColorItem() { Color = Color.Red, Name = "Red" },
            new ColorItem() { Color = Color.RosyBrown, Name = "RosyBrown" },
            new ColorItem() { Color = Color.SlateBlue, Name = "SlateBlue" },
            new ColorItem() { Color = Color.RoyalBlue, Name = "RoyalBlue" },
            new ColorItem() { Color = Color.Salmon, Name = "Salmon" },
            new ColorItem() { Color = Color.SandyBrown, Name = "SandyBrown" },
            new ColorItem() { Color = Color.SeaGreen, Name = "SeaGreen" },
            new ColorItem() { Color = Color.SeaShell, Name = "SeaShell" },
            new ColorItem() { Color = Color.Sienna, Name = "Sienna" },
            new ColorItem() { Color = Color.Silver, Name = "Silver" },
            new ColorItem() { Color = Color.SaddleBrown, Name = "SaddleBrown" },
            new ColorItem() { Color = Color.LightSalmon, Name = "LightSalmon" },
            new ColorItem() { Color = Color.MediumPurple, Name = "MediumPurple" },
            new ColorItem() { Color = Color.LightGreen, Name = "LightGreen" },
            new ColorItem() { Color = Color.Crimson, Name = "Crimson" },
            new ColorItem() { Color = Color.Cyan, Name = "Cyan" },
            new ColorItem() { Color = Color.LightPink, Name = "LightPink" },
            new ColorItem() { Color = Color.DarkCyan, Name = "DarkCyan" },
            new ColorItem() { Color = Color.DarkGoldenrod, Name = "DarkGoldenrod" },
            new ColorItem() { Color = Color.DarkGray, Name = "DarkGray" },
            new ColorItem() { Color = Color.Cornsilk, Name = "Cornsilk" },
            new ColorItem() { Color = Color.DarkGreen, Name = "DarkGreen" },
            new ColorItem() { Color = Color.DarkMagenta, Name = "DarkMagenta" },
            new ColorItem() { Color = Color.DarkOliveGreen, Name = "DarkOliveGreen" },
            new ColorItem() { Color = Color.DarkOrange, Name = "DarkOrange" },
            new ColorItem() { Color = Color.DarkOrchid, Name = "DarkOrchid" },
            new ColorItem() { Color = Color.DarkRed, Name = "DarkRed" },
            new ColorItem() { Color = Color.DarkSalmon, Name = "DarkSalmon" },
            new ColorItem() { Color = Color.DarkKhaki, Name = "DarkKhaki" },
            new ColorItem() { Color = Color.DarkSeaGreen, Name = "DarkSeaGreen" },
            new ColorItem() { Color = Color.CornflowerBlue, Name = "CornflowerBlue" },
            new ColorItem() { Color = Color.Chocolate, Name = "Chocolate" },
            new ColorItem() { Color = Color.AntiqueWhite, Name = "AntiqueWhite" },
            new ColorItem() { Color = Color.Aqua, Name = "Aqua" },
            new ColorItem() { Color = Color.Aquamarine, Name = "Aquamarine" },
            new ColorItem() { Color = Color.Azure, Name = "Azure" },
            new ColorItem() { Color = Color.Beige, Name = "Beige" },
            new ColorItem() { Color = Color.Bisque, Name = "Bisque" },
            new ColorItem() { Color = Color.Coral, Name = "Coral" },
            new ColorItem() { Color = Color.Black, Name = "Black" },
            new ColorItem() { Color = Color.Blue, Name = "Blue" },
            new ColorItem() { Color = Color.BlueViolet, Name = "BlueViolet" },
            new ColorItem() { Color = Color.Brown, Name = "Brown" },
            new ColorItem() { Color = Color.BurlyWood, Name = "BurlyWood" },
            new ColorItem() { Color = Color.CadetBlue, Name = "CadetBlue" },
            new ColorItem() { Color = Color.Chartreuse, Name = "Chartreuse" },
            new ColorItem() { Color = Color.BlanchedAlmond, Name = "BlanchedAlmond" },
            new ColorItem() { Color = Color.DarkSlateBlue, Name = "DarkSlateBlue" },
            new ColorItem() { Color = Color.DarkBlue, Name = "DarkBlue" },
            new ColorItem() { Color = Color.DarkTurquoise, Name = "DarkTurquoise" },
            new ColorItem() { Color = Color.HotPink, Name = "HotPink" },
            new ColorItem() { Color = Color.IndianRed, Name = "IndianRed" },
            new ColorItem() { Color = Color.Indigo, Name = "Indigo" },
            new ColorItem() { Color = Color.Ivory, Name = "Ivory" },
            new ColorItem() { Color = Color.Khaki, Name = "Khaki" },
            new ColorItem() { Color = Color.Lavender, Name = "Lavender" },
            new ColorItem() { Color = Color.Honeydew, Name = "Honeydew" },
            new ColorItem() { Color = Color.LavenderBlush, Name = "LavenderBlush" },
            new ColorItem() { Color = Color.LemonChiffon, Name = "LemonChiffon" },
            new ColorItem() { Color = Color.LightBlue, Name = "LightBlue" },
            new ColorItem() { Color = Color.LightCoral, Name = "LightCoral" },
            new ColorItem() { Color = Color.DarkSlateGray, Name = "DarkSlateGray" },
            new ColorItem() { Color = Color.LightGoldenrodYellow, Name = "LightGoldenrodYellow" },
            new ColorItem() { Color = Color.LightGray, Name = "LightGray" },
            new ColorItem() { Color = Color.Gray, Name = "Gray" },
            new ColorItem() { Color = Color.Green, Name = "Green" },
            new ColorItem() { Color = Color.DarkViolet, Name = "DarkViolet" },
            new ColorItem() { Color = Color.DeepPink, Name = "DeepPink" },
            new ColorItem() { Color = Color.DeepSkyBlue, Name = "DeepSkyBlue" },
            new ColorItem() { Color = Color.DodgerBlue, Name = "DodgerBlue" },
            new ColorItem() { Color = Color.Firebrick, Name = "Firebrick" },
            new ColorItem() { Color = Color.FloralWhite, Name = "FloralWhite" },
            new ColorItem() { Color = Color.DimGray, Name = "DimGray" },
            new ColorItem() { Color = Color.Fuchsia, Name = "Fuchsia" },
            new ColorItem() { Color = Color.Gainsboro, Name = "Gainsboro" },
            new ColorItem() { Color = Color.Goldenrod, Name = "Goldenrod" },
            new ColorItem() { Color = Color.GhostWhite, Name = "GhostWhite" },
            new ColorItem() { Color = Color.Gold, Name = "Gold" },
            new ColorItem() { Color = Color.ForestGreen, Name = "ForestGreen" }
        };

        public static IList<ColorItem> GetSystemColorList()
        {
            return _systemColorItems;
        }

        public static ColorItem RandomColor()
        {
#pragma warning disable CA5394 // Do not use insecure randomness
            return _systemColorItems[new Random().Next(0, _systemColorItems.Count)];
#pragma warning restore CA5394 // Do not use insecure randomness
        }
    }
}
