﻿
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace HB.FullStack.Database.Convert
{
    public class GuidListTypeConverter : IDbValueConverter
    {
        public DbType DbType => DbType.String;

        public string DbTypeStatement => "VARCHAR";

        public object DbValueToTypeValue(object dbValue, Type propertyType)
        {
            string? str = dbValue.ToString();

            if (str.IsNotNullOrEmpty())
            {
                return new ObservableCollection<Guid>(str.Split(',').Select(i => new Guid(i)).ToList());
            }

            return new ObservableCollection<Guid>();
        }

        public object TypeValueToDbValue(object typeValue, Type propertyType)
        {
            if (typeValue is IEnumerable<Guid> guids)
            {
                return guids.ToJoinedString(",");
            }

            throw DatabaseExceptions.TypeConverterError("使用LongIdListTypeConverter的，必须可赋值为IEnumerable<Guid>", propertyType.FullName);
        }
    }
}