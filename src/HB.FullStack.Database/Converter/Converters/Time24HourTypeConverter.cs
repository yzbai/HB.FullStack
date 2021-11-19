﻿using System;
using System.Data;

using HB.FullStack.Common;

namespace HB.FullStack.Database.Converter
{
    public class Time24HourTypeConverter : ITypeConverter
    {
        public DbType DbType => DbType.String;

        public string Statement => "VARCHAR(10)";

        public object DbValueToTypeValue(object dbValue, Type propertyType)
        {
            return new Time24Hour(dbValue.ToString()!);
        }

        public object TypeValueToDbValue(object typeValue, Type propertyType)
        {
            return ((Time24Hour)typeValue).ToString();
        }
    }
}