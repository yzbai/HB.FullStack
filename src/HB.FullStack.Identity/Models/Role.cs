using HB.FullStack.Common.Shared;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Server.Identity.Models
{
    public class Role : TimestampGuidDbModel
    {
        [DbField(Unique = true, NotNull = true)]
        public string Name { get; set; } = default!;

        [DbField(NotNull = true)]
        public string DisplayName { get; set; } = default!;

        public bool IsActivated { get; set; }

        [DbField(MaxLength = SharedNames.Length.MAX_ROLE_COMMENT_LENGTH)]
        public string? Comment { get; set; }

        public void Update(string name, string displayName, bool isActivated, string? comment)
        {
            Name = name;
            DisplayName = displayName;
            IsActivated = isActivated;
            Comment = comment;
        }
    }
}