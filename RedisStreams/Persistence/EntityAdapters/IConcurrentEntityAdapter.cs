using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisStreams.Persistence.EntityAdapters
{
	interface IConcurrentEntityAdapter<TEntity>  where TEntity : class
	{
		Task<bool> SetAsync(ITransaction tran,RedisKey key, TEntity entity, TimeSpan? expiry, string? expectedConcurrencyToken, string newConcurrencyToken);
		Task<bool> SetAsync(ITransaction tran, RedisKey key, TEntity entity, TimeSpan? expiry, string? expectedConcurrencyToken, string newConcurrencyToken, string concurrencyTokenKey);
		Task<bool> RemoveAsync(ITransaction tran, RedisKey key, string? expectedConcurrencyToken);
		Task<bool> RemoveAsync(ITransaction tran, RedisKey key, string? expectedConcurrencyToken, string concurrencyTokenKey);
	}
}
