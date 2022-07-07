using HB.FullStack.Database.DatabaseModels;

namespace HB.FullStack.Identity.Models
{
    public class Role : GuidDatabaseModel
    {
        [DatabaseModelProperty(Unique = true, NotNull = true)]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty(NotNull = true)]
        public string DisplayName { get; set; } = default!;

        public bool IsActivated { get; set; }

        [DatabaseModelProperty(MaxLength = LengthConventions.MAX_ROLE_COMMENT_LENGTH)]
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