using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisStreams.Persistence.EntityAdapters
{
	interface IBaseEntityAdapter<TEntity> where TEntity : class
	{
		Task<bool> KeyExists(RedisKey key);
		Task<TEntity?> TryGet(RedisKey key);
	}
}
