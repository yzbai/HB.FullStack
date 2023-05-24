/*
 * Author：Yuzhao Bai
 * Email: yzbai@brlite.com
 * Github: github.com/yzbai
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using CommunityToolkit.Mvvm.ComponentModel;

using HB.FullStack.Common;
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

    [PropertyTrackableObject]
    public partial class UPTimestampModel : DbModel<Guid>, ITimestamp
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

        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    [PropertyTrackableObject]
    public partial class UPTimelessModel : DbModel<Guid>
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

        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
    }
}