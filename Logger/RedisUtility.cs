using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Logger
{
    public class RedisUtility
    {
        private readonly IDatabase _database;

        public RedisUtility(IDatabase database)
        {
            _database = database;
        }

        public static string KeyGen(string key, Type T)
        {
            string fullKey = $"{T.FullName}:{key}";
            return fullKey;
        }

        public async Task DbSetAsync<T>(string key, T value)
        {
            await _database.StringSetAsync(KeyGen(key, typeof(T)), JsonConvert.SerializeObject(value));
        }

        public async Task<T> DbGetAsync<T>(string key)
        {
            if(!await _database.KeyExistsAsync(KeyGen(key, typeof(T))))
            {
                return default(T);
            }

            var str = await _database.StringGetAsync(KeyGen(key, typeof(T)));
            var result = JsonConvert.DeserializeObject<T>(str.ToString().Replace("\uFEFF", ""));
            return result;
        }

        public async Task DbDelAsync<T>(string key)
        {
            await _database.KeyDeleteAsync(KeyGen(key, typeof(T)));
        }
    }
}
