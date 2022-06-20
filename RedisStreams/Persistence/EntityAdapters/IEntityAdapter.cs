using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisStreams.Persistence.EntityAdapters
{
	interface IEntityAdapter<TEntity> : IBaseEntityAdapter<TEntity> where TEntity : class
	{
		bool Set(IDatabase db, RedisKey key, TEntity entity, TimeSpan? expiry);
		Task<bool> SetAsync(IDatabase db, RedisKey key, TEntity entity, TimeSpan? expiry);

		bool Remove(IDatabase db, RedisKey key);
		Task<bool> RemoveAsync(IDatabase db, RedisKey key);
	}
}
