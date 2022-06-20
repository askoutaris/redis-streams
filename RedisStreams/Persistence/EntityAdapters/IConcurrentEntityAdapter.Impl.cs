using System;
using System.Threading.Tasks;
using RedisStreams.Serializers;
using StackExchange.Redis;

namespace RedisStreams.Persistence.EntityAdapters
{
	class ConcurrentEntityAdapter<TEntity> : BaseEntityAdapter<TEntity>, IConcurrentEntityAdapter<TEntity> where TEntity : class
	{
		public ConcurrentEntityAdapter(string moduleName, IRedisSerializer<TEntity> serializer) : base(moduleName, serializer)
		{
		}

		public Task<bool> SetAsync(ITransaction tran, RedisKey key, TEntity entity, TimeSpan? expiry, string? expectedConcurrencyToken, string newConcurrencyToken)
		{
			var concurrencyTokenKey = GetConcurrencyTokenKey(key);

			return SetAsync(tran, key, entity, expiry, expectedConcurrencyToken, newConcurrencyToken, concurrencyTokenKey);
		}

		public Task<bool> SetAsync(ITransaction tran, RedisKey key, TEntity entity, TimeSpan? expiry, string? expectedConcurrencyToken, string newConcurrencyToken, string concurrencyTokenKey)
		{
			EnsureConcurrency(tran, concurrencyTokenKey, expectedConcurrencyToken);

			tran.StringSetAsync(concurrencyTokenKey, newConcurrencyToken);

			return tran.StringSetAsync(
				key: GetEntityKey(key),
				value: _serializer.Serialize(entity),
				expiry: expiry);
		}

		public Task<bool> RemoveAsync(ITransaction tran, RedisKey key, string? expectedConcurrencyToken)
		{
			var concurrencyTokenKey = GetConcurrencyTokenKey(key);

			return RemoveAsync(tran, key, expectedConcurrencyToken, concurrencyTokenKey);
		}

		public Task<bool> RemoveAsync(ITransaction tran, RedisKey key, string? expectedConcurrencyToken, string concurrencyTokenKey)
		{
			EnsureConcurrency(tran, concurrencyTokenKey, expectedConcurrencyToken);

			tran.KeyDeleteAsync(concurrencyTokenKey);

			return tran.KeyDeleteAsync(key: GetEntityKey(key.ToString()));
		}

		private void EnsureConcurrency(ITransaction tran, string concurrencyTokenKey, string? expectedConcurrencyToken)
		{
			if (expectedConcurrencyToken is null)
				tran.AddCondition(Condition.KeyNotExists(concurrencyTokenKey));
			else
				tran.AddCondition(Condition.StringEqual(concurrencyTokenKey, expectedConcurrencyToken));
		}

		private string GetConcurrencyTokenKey(RedisKey key)
			=> $"{Constants.Prefix}.{_moduleName}.{typeof(TEntity).Name}Version#{key}";
	}
}
