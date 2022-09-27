using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client.ClientModels
{
    public class ClientDbModel : TimelessGuidDbModel
    {
        #region Changed Properties

        private readonly List<ChangedProperty> _changedProperties = new List<ChangedProperty>();

        public IList<ChangedProperty> GetChangedProperties() { return _changedProperties; }

        public void ClearChangedProperties()
        {
            _changedProperties.Clear();
        }

        protected bool SetProperty<T>([NotNullIfNotNull("newValue")] ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return false;
            }

            ChangedProperty changedProperty = new ChangedProperty
            {
                Name = propertyName!,
                OldValue = field,
                NewValue = newValue
            };

            field = newValue;
            _changedProperties.Add(changedProperty);

            return true;
        }

        #endregion

        /// <summary>
        /// 改动时间，包括：
        /// 1. Update
        /// 2. Get from network
        /// </summary>
        public DateTimeOffset UpdatedTime { get; set; } = DateTimeOffset.UtcNow;

        private string? _name;

        public string? Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
    }
}
