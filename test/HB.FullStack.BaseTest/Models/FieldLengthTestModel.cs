/*
 * Author：Yuzhao Bai
 * Email: yzbai@brlite.com
 * Github: github.com/yzbai
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using HB.FullStack.Common;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Models
{
    public class FieldLengthTestModel : DbModel2<long>, ITimestamp
    {
        [DbField(MaxLength = 10)]
        public string? Content { get; set; }

        [DbAutoIncrementPrimaryKey]
        public override long Id { get; set; }

        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }
}