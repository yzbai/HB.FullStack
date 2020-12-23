//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Text;

//namespace System.ComponentModel.DataAnnotations
//{
//    public sealed class ImageFileAttribute : ValidationAttribute
//    {
//        public int Length { get; set; }

//        public string[] ImageType { get; set; }

//        public ImageFileAttribute()
//        {
//            if (string.IsNullOrEmpty(ErrorMessage))
//            {
//                ErrorMessage = "这不是一张合格的图片文件啊";
//            }

//            Length = 1048576; //1MB

//            ImageType = new string[] { ".png", ".jpg", ".gif" };
//        }

//        public override bool IsValid(object value)
//        {
//            if (!(value is IFormFile file))
//            {
//                ErrorMessage = "不是图片文件";
//                return false;
//            }

//            if (file.Length == 0)
//            {
//                ErrorMessage = "图片是空的";
//                return false;
//            }

//            if (file.Length > Length)
//            {
//                ErrorMessage = "图片太大";
//                return false;
//            }

//            string fileType = MediaUtil.GetFileTypeByMediaType(mediaType: file.ContentType);

//            if (!fileType.IsIn(ImageType))
//            {
//                ErrorMessage = "不是合适的图片类型";
//                return false;
//            }

//            return true;
//        }
//    }
//}