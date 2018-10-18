using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.Framework.Common.Entity
{
    /// <summary>
    /// 基础领域模型
    /// 内建验证机制。
    /// </summary>
    public class CommonEntity : ISupportValidate
    {
        #region Validation

        private readonly IList<ValidationResult> __validateResults = null;

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

        private bool __performValidate()
        {
            ValidationContext vContext = new ValidationContext(this);
            return Validator.TryValidateObject(this, vContext, __validateResults, true);
        }

        #endregion
    }
}
