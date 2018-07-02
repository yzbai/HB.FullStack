namespace System.ComponentModel.DataAnnotations
{
    public class PasswordAttribute : ValidationAttribute
    {
        public PasswordAttribute()
        {
            if (string.IsNullOrEmpty(ErrorMessage))
                ErrorMessage = "xxxx";
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }
            string str = value.ToString();

            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return HB.Framework.Common.Validate.ValidationMethods.IsPassword(str);
        }
    }
}
