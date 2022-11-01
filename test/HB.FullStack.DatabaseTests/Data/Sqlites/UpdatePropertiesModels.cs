﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.DatabaseTests.Data.Sqlites
{
    [DbModel(DbSchema_Sqlite)]
    public partial class InnerModel : ObservableObject
    {
        [ObservableProperty]
        private string? _innerName;
    }

    [DbModel(DbSchema_Sqlite)]
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

    [DbModel(DbSchema_Sqlite)]
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
