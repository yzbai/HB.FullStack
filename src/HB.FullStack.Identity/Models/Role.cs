using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Identity.Models
{
    public class Role : TimestampGuidDbModel
    {
        [DBModelProperty(Unique = true, NotNull = true)]
        public string Name { get; set; } = default!;

        [DBModelProperty(NotNull = true)]
        public string DisplayName { get; set; } = default!;

        public bool IsActivated { get; set; }

        [DBModelProperty(MaxLength = LengthConventions.MAX_ROLE_COMMENT_LENGTH)]
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