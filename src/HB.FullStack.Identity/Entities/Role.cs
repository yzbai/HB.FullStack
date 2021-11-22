using HB.FullStack.Database.Entities;

namespace HB.FullStack.Identity.Entities
{
    public class Role : GuidEntity
    {
        [EntityProperty(Unique = true, NotNull = true)]
        public string Name { get; set; } = default!;

        [EntityProperty(NotNull = true)]
        public string DisplayName { get; set; } = default!;

        public bool IsActivated { get; set; }

        [EntityProperty(MaxLength = LengthConventions.MAX_ROLE_COMMENT_LENGTH)]
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