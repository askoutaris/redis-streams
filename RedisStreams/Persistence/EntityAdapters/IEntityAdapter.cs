using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisStreams.Persistence.EntityAdapters
{
	interface IEntityAdapter<TEntity> : IBaseEntityAdapter<TEntity> where TEntity : class
	{
		bool Set(RedisKey key, TEntity entity, TimeSpan? expiry);
		Task<bool> SetAsync(RedisKey key, TEntity entity, TimeSpan? expiry);

		bool Remove(RedisKey key);
		Task<bool> RemoveAsync(RedisKey key);
	}
}
