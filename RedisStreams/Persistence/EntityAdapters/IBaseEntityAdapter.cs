using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisStreams.Persistence.EntityAdapters
{
	interface IBaseEntityAdapter<TEntity> where TEntity : class
	{
		Task<bool> KeyExists(IDatabase db, RedisKey key);
		Task<TEntity?> TryGet(IDatabase db, RedisKey key);
	}
}
