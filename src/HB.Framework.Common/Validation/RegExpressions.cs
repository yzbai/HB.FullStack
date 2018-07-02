using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HB.Framework.Common.Validate
{
    //TODO: 补充正则表达式
    public static class RegExpressions
    {
        public static readonly string Url = "^(https?://)"
                + "?(([0-9a-z_!~*'().&=+$%-]+: )?[0-9a-z_!~*'().&=+$%-]+@)?" //user@
                + @"(([0-9]{1,3}\.){3}[0-9]{1,3}" // IP- 199.194.52.184
                + "|" // allows either IP or domain
                + @"([0-9a-z_!~*'()-]+\.)*" // tertiary domain(s)- www.
                + @"([0-9a-z][0-9a-z-]{0,61})?[0-9a-z]" // second level domain
                + @"(\.[a-z]{2,6})?)" // first level domain- .com or .museum is optional
                + "(:[0-9]{1,5})?" // port number- :80
                + "((/?)|" // a slash isn't required if there is no file name
                + "(/[0-9a-z_!~*'().;?:@&=+$,%#-]+)+/?)$";

        public static readonly string Email
            //= @"^[a-z0-9_\-]+(\.[_a-z0-9\-]+)*@([_a-z0-9\-]+\.)+([a-z]{2}|aero|arpa|biz|com|coop|edu|gov|info|int|jobs|mil|museum|name|nato|net|org|pro|travel)$";
            = @"^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$";

        public static readonly string MobilePhone
            //= @"^((\(\d{3}\))|(\d{3}\-))?1\d{10}$";
            = @"^0?(13|15|18|14)[0-9]{9}$";

        public static readonly string TelePhone
            //= @"^(\d{3.4}-)\d{7,8}$"; //@"^([0-9]{3}|0[0-9]{3})-[1-9][0-9]{6,7}(-[0-9]{1,6})?$";
            = @"^[0-9\-()（）]{7,18}$";

        public static readonly string Integer
            = @"^-?[1-9]\d*$";

        public static readonly string PositiveInteger
            = @"^[1-9]\d*$";

        public static readonly string NegativeInteger
            = @"^-[1-9]\d*$";

        public static readonly string Number
            = @"^([+-]?)\d*\.?\d+$";
          
        public static readonly string PositiveNumber
            = @"^[1-9]\d*|0$";

        public static readonly string NegativeNumber
            = @"^-[1-9]\d*|0$";

        public static readonly string Ascii
            = @"^[\x00-\xFF]+$";

        public static readonly string Chinese
            = @"^[\u4e00-\u9fa5]+$";

        public static readonly string Letter
            = @"^[A-Za-z]+$";

        public static readonly string LowerLetter
            = @"^[a-z]+$";

        public static readonly string UpperLetter
            = @"^[A-Z]+$";

        public static readonly string LoginName
            = @"^[A-Za-z0-9_\-\u4e00-\u9fa5]+$";

        public static readonly string NickName
            = @"^[A-Za-z0-9_\-\u4e00-\u9fa5]+$";

        public static readonly string Password
            = @"^.*[A-Za-z0-9\w_-]+.*$";

        public static readonly string Day
            = @"^((0?[1-9])|((1|2)[0-9])|30|31)$";

        public static readonly string Year
            =@"^[1-2][0-9][0-9][0-9]";

        public static readonly string Month
            = @"^(0?[1-9]|1[0-2])$";
    }
}



