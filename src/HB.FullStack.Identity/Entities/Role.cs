using HB.FullStack.Database.Entities;

using MessagePack;

namespace HB.FullStack.Identity.Entities
{
    [MessagePackObject]
    public class Role : GuidEntity
    {
        [EntityProperty(Unique = true, NotNull = true)]
        [Key(7)]
        public string Name { get; set; } = default!;

        [EntityProperty(NotNull = true)]
        [Key(8)]
        public string DisplayName { get; set; } = default!;

        [Key(9)]
        public bool IsActivated { get; set; }

        [EntityProperty(MaxLength = LengthConventions.MAX_ROLE_COMMENT_LENGTH)]
        [Key(10)]
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