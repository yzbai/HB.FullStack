﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.DatabaseTests.Data.MySqls
{
    public record InnerModel(string? InnerName);

    [DbModel(DbSchema_Mysql)]
    [PropertyTrackableObject]
    public partial class UPTimestampModel : TimestampGuidDbModel
    {
        [TrackProperty]
        private string? _name;

        [TrackProperty]
        private int? _age;

        [TrackProperty]
        [DbModelProperty(Converter = typeof(JsonDbPropertyConverter))]
        private InnerModel? _innerModel;

    }

    [DbModel(DbSchema_Mysql)]
    [PropertyTrackableObject]
    public partial class UPTimelessModel : TimelessGuidDbModel
    {
        [TrackProperty]
        private string? _name;

        [TrackProperty]
        private int? _age;

        [TrackProperty]
        [DbModelProperty(Converter = typeof(JsonDbPropertyConverter))]
        private InnerModel? _innerModel;
    }
}
