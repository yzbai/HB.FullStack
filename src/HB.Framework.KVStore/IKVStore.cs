using HB.Framework.Common.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;



namespace HB.Framework.KVStore
{
    public interface IKVStore
    {
        Task<T?> GetAsync<T>(string key) where T : Entity, new();
        Task<IEnumerable<T?>> GetAsync<T>(IEnumerable<string> keys) where T : Entity, new();

        Task<IEnumerable<T?>> GetAllAsync<T>() where T : Entity, new();

        public Task AddAsync<T>(T item, string lastUser) where T : Entity, new();

        /// <summary>
        /// 反应Version变化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="lastUser"></param>
        /// <returns></returns>
        Task AddAsync<T>(IEnumerable<T> items, string lastUser) where T : Entity, new();

        public Task UpdateAsync<T>(T item, string lastUser) where T : Entity, new();

        /// <summary>
        /// 反应Version变化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="lastUser"></param>
        /// <returns></returns>
        Task UpdateAsync<T>(IEnumerable<T> items, string lastUser) where T : Entity, new();

        

        public Task DeleteAsync<T>(string key, int version) where T : Entity, new();

        Task DeleteAsync<T>(IEnumerable<string> keys, IEnumerable<int> versions) where T : Entity, new();


        Task DeleteAllAsync<T>() where T : Entity, new();

        /// <summary>
        /// 返回最新Version
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="lastUser"></param>
        /// <returns></returns>
        Task<int> AddOrUpdateAsync<T>(T item, string lastUser) where T : Entity, new();

        /// <summary>
        /// 返回最新的Versions
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="lastUser"></param>
        /// <returns></returns>
        Task<IEnumerable<int>> AddOrUpdateAsync<T>(IEnumerable<T> items, string lastUser) where T : Entity, new();

        string GetEntityKey<T>(T item) where T : Entity, new();
    }
}