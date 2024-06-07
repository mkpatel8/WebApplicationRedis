using Newtonsoft.Json;
using StackExchange.Redis;
using WebApplicationRedis.Models;

namespace WebApplicationRedis.Services
{
    public class RedisCacheService
    {
        private readonly IDatabase _redisDatabase;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _redisDatabase = redis.GetDatabase();
        }

        private string GetCacheKey(string userId)
        {
            return $"user:{userId}";
        }

        public async Task<UserData> GetUserDataAsync(string userId)
        {
            var cacheKey = GetCacheKey(userId);
            var hashEntries = await _redisDatabase.HashGetAllAsync(cacheKey);

            if (hashEntries.Length == 0)
            {
                return null; // Indicate cache miss
            }

            var userData = new UserData
            {
                UserId = userId,
                FirstName = hashEntries.FirstOrDefault(x => x.Name == "FirstName").Value,
                LastName = hashEntries.FirstOrDefault(x => x.Name == "LastName").Value,
                Age = (int)hashEntries.FirstOrDefault(x => x.Name == "Age").Value
            };

            return userData;
        }

        public async Task SetUserDataAsync(string userId, UserData userData)
        {
            var cacheKey = GetCacheKey(userId);
            var hashEntries = new HashEntry[]
            {
                new HashEntry("UserId", userData.UserId),
                new HashEntry("FirstName", userData.FirstName),
                new HashEntry("LastName", userData.LastName),
                new HashEntry("Age", userData.Age)
            };

            await _redisDatabase.HashSetAsync(cacheKey, hashEntries);
            await _redisDatabase.KeyExpireAsync(cacheKey, TimeSpan.FromHours(1)); // Set expiration time
        }

        public async Task SetUserDetailsBatchAsync(IEnumerable<UserData> users)
        {
            var batch = _redisDatabase.CreateBatch();
            var tasks = new List<Task>();

            foreach (var user in users)
            {
                var cacheKey = GetCacheKey(user.UserId);
                var hashEntries = new HashEntry[]
                {
                    new HashEntry("UserId", user.UserId),
                    new HashEntry("FirstName", user.FirstName),
                    new HashEntry("LastName", user.LastName),
                    new HashEntry("Age", user.Age)
                };
                tasks.Add(batch.HashSetAsync(cacheKey, hashEntries));
            }

            batch.Execute();
            await Task.WhenAll(tasks);
        }
    }
}
