using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrmBenchmark.Core
{
    public interface IOrmExecuter
    {
        string Name { get; }
        void Init(string connectionStrong);

        dynamic GetItemAsDynamic(int Id);

        IEnumerable<dynamic> GetAllItemsAsDynamic();
        void Finish();
        Task<IPost> GetItemAsObjectAsync(int Id);
        Task<IEnumerable<IPost>> GetAllItemsAsObjectAsync();
    }
}
