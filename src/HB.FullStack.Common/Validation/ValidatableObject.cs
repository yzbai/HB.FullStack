#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace HB.FullStack.Common
{
    //TODO: 考虑验证嵌套类， 和 集合类
    //asp.net core model binding 可以验证嵌套类，但无法验证集合类
    /// <summary>
    /// 基础领域模型
    /// 内建验证机制。
    /// 不能应对嵌套的类的验证
    /// </summary>
    public class ValidatableObject : ISupportValidate
    {
        #region Validation

        private IList<ValidationResult>? _validateResults;
        private ValidationContext? _validationContext;

        public bool IsValid()
        {
            return PerformValidate();
        }

        public IList<ValidationResult> GetValidateResults(bool forced = false)
        {
            if (_validateResults == null || forced)
            {
                PerformValidate();
            }
            return _validateResults!;
        }

        public string GetValidateErrorMessage()
        {
            if (_validateResults == null)
            {
                PerformValidate();
            }

            StringBuilder builder = new StringBuilder();

            foreach (ValidationResult result in _validateResults!)
            {
                builder.AppendLine(result.ErrorMessage);
            }

            return builder.ToString();
        }

        public bool PerformValidate(string? propertyName = null)
        {
            try
            {
                _validateResults = new List<ValidationResult>();

                if (_validationContext == null)
                {
                    _validationContext = new ValidationContext(this);
                }

                if (!string.IsNullOrEmpty(propertyName))
                {
                    _validationContext.MemberName = propertyName;

                    PropertyInfo? propertyInfo = GetType().GetProperty(propertyName);

                    if (propertyInfo != null)
                    {
                        object? propertyValue = propertyInfo.GetValue(this);

                        return Validator.TryValidateProperty(propertyValue, _validationContext, _validateResults);
                    }
                }

                bool result = Validator.TryValidateObject(this, _validationContext, _validateResults, true);

                return result;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                GlobalSettings.Logger?.LogPerformValidateError(propertyName, ex);
                return false;
            }
        }

        #endregion Validation
    }
}

#nullable restore