using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HB.Framework.Common.Entity
{
    /// <summary>
    /// 基础领域模型
    /// 内建验证机制。
    /// </summary>
    public class CommonEntity : ISupportValidate
    {
        #region Validation

        private readonly IList<ValidationResult> __validateResults = new List<ValidationResult>();

        public bool IsValid()
        {
            return __performValidate();
        }

        public IList<ValidationResult> GetValidateResults()
        {
            if (__validateResults == null)
            {
                __performValidate();
            }
            return __validateResults;
        }

        public string GetValidateErrorMessage()
        {
            StringBuilder builder = new StringBuilder();

            foreach (ValidationResult result in __validateResults)
            {
                builder.AppendLine(result.ErrorMessage);
            }

            return builder.ToString();
        }

        private bool __performValidate()
        {
            ValidationContext vContext = new ValidationContext(this);
            return Validator.TryValidateObject(this, vContext, __validateResults, true);
        }

        #endregion
    }
}
