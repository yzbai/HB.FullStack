namespace System.ComponentModel.DataAnnotations
{
    public class MobileAttribute : ValidationAttribute
    {
        public MobileAttribute()
        {
            if (string.IsNullOrEmpty(ErrorMessage))
                ErrorMessage = "xxxxxxxxxxxxxxxxxxxx";
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            string str = value as string;

            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return HB.Framework.Common.Validate.ValidationMethods.IsMobilePhone(str);
        }
    }
}
