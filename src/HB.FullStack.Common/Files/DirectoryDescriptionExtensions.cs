using System;

using HB.FullStack.Common.Files;

namespace System
{
    public static class DirectoryDescriptionExtensions
    {
        public static string GetPath(this DirectoryDescription directoryDescription, string? placeHoderValue)
        {
            if(directoryDescription.IsPathContainsPlaceHolder)
            {
                if(placeHoderValue.IsNullOrEmpty())
                {
                    throw new ArgumentNullException(nameof(placeHoderValue));
                }

                return directoryDescription.DirectoryPath.Replace(directoryDescription.PlaceHolderName!, placeHoderValue, StringComparison.Ordinal);
            }

            return directoryDescription.DirectoryPath;
        }

        public static Directory2 ToDirectory(this DirectoryDescription directoryDescription, string? placeHolderValue)
        {
            return new Directory2 { DirectoryName = directoryDescription.DirectoryName, PlaceHolderValue = placeHolderValue };
        }
    }
}
