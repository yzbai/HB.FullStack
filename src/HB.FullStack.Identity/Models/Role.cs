using HB.FullStack.Common;
using HB.FullStack.Common.Shared;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Server.Identity.Models
{
    //public interface IRole : IModel
    //{
    //    object Id { get; }

    //    string? Comment { get; set; }
    //    string DisplayName { get; set; }
    //    bool IsActivated { get; set; }
    //    string Name { get; set; }

    //    void Update(string name, string displayName, bool isActivated, string? comment);
    //}

    [DbModel(ConflictCheckMethods = ConflictCheckMethods.Timestamp)]
    public class Role<TId> : DbModel<TId>, ITimestamp
    {
        [DbField(Unique = true, NotNull = true)]
        public string Name { get; set; } = default!;

        [DbField(NotNull = true)]
        public string DisplayName { get; set; } = default!;

        public bool IsActivated { get; set; }

        [DbField(MaxLength = SharedNames.Length.MAX_ROLE_COMMENT_LENGTH)]
        public string? Comment { get; set; }

        public override TId Id { get; set; } = default!;
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
        
        public void Update(string name, string displayName, bool isActivated, string? comment)
        {
            Name = name;
            DisplayName = displayName;
            IsActivated = isActivated;
            Comment = comment;
        }
    }
}