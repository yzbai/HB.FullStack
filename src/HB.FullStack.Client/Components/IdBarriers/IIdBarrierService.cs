using System;

namespace HB.FullStack.Client.Components.IdBarriers
{
    /// <summary>
    /// 很不错的设想，来自trello。https://www.atlassian.com/engineering/sync-two-id-problem
    /// 实施起来效率是个问题，bug也很容易产生。所以直接用Guid了。此模块保留仅供参考
    /// </summary>
    public interface IIdBarrierService
    {


        void Initialize();
    }
}