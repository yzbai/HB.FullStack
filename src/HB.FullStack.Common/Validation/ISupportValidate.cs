

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common
{
    /// <summary>
    /// 内建验证机制接口
    /// </summary>
    public interface ISupportValidate
    {
        /// <summary>
        /// 是否通过验证
        /// </summary>
        bool IsValid();

        /// <summary>
        /// 获取验证结果。用foeach
        /// </summary>
        IList<ValidationResult> GetValidateResults(bool forced = false);

        string GetValidateErrorMessage();

        bool PerformValidate(string? propertyName = null);
    }
}