using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Data.MySqls
{
    public record InnerModel(string? InnerName);


    public partial class InnerModel2 : ObservableObject
    {
        [ObservableProperty]
        public string? _innerName;
    }

    [DbModel(DbSchema_Mysql)]
    [PropertyTrackableObject]
    public partial class UPTimestampModel : TimestampGuidDbModel
    {
        [TrackProperty]
        private string? _name;

        [TrackProperty]
        private int? _age;

        [TrackProperty]
        [DbField(Converter = typeof(JsonDbPropertyConverter))]
        private InnerModel? _innerModel;

        [TrackProperty]
        [DbField(Converter = typeof(JsonDbPropertyConverter))]
        private InnerModel2? _innerModel2;

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
        [DbField(Converter = typeof(JsonDbPropertyConverter))]
        private InnerModel? _innerModel;

        [TrackProperty]
        [DbField(Converter = typeof(JsonDbPropertyConverter))]
        private InnerModel2? _innerModel2;
    }
}
