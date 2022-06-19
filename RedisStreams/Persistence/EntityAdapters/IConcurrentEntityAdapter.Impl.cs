using System;
using System.Threading.Tasks;
using RedisStreams.Serializers;
using StackExchange.Redis;

namespace RedisStreams.Persistence.EntityAdapters
{
	class ConcurrentEntityAdapter<TEntity> : BaseEntityAdapter<TEntity>, IConcurrentEntityAdapter<TEntity> where TEntity : class
	{
		private readonly ITransaction _transaction;

		public ConcurrentEntityAdapter(string moduleName, ITransaction transaction, IRedisSerializer<TEntity> serializer) : base(moduleName, transaction, serializer)
		{
			_transaction = transaction;
		}

		public Task<bool> SetAsync(RedisKey key, TEntity entity, TimeSpan? expiry, string? expectedConcurrencyToken, string newConcurrencyToken)
		{
			var concurrencyTokenKey = GetConcurrencyTokenKey(key);

			return SetAsync(key, entity, expiry, expectedConcurrencyToken, newConcurrencyToken, concurrencyTokenKey);
		}

		public Task<bool> SetAsync(RedisKey key, TEntity entity, TimeSpan? expiry, string? expectedConcurrencyToken, string newConcurrencyToken, string concurrencyTokenKey)
		{
			EnsureConcurrency(concurrencyTokenKey, expectedConcurrencyToken);

			_transaction.StringSetAsync(concurrencyTokenKey, newConcurrencyToken);

			return _transaction.StringSetAsync(
				key: GetEntityKey(key),
				value: _serializer.Serialize(entity),
				expiry: expiry);
		}

		public Task<bool> RemoveAsync(RedisKey key, string? expectedConcurrencyToken)
		{
			var concurrencyTokenKey = GetConcurrencyTokenKey(key);

			return RemoveAsync(key, expectedConcurrencyToken, concurrencyTokenKey);
		}

		public Task<bool> RemoveAsync(RedisKey key, string? expectedConcurrencyToken, string concurrencyTokenKey)
		{
			EnsureConcurrency(concurrencyTokenKey, expectedConcurrencyToken);

			_transaction.KeyDeleteAsync(concurrencyTokenKey);

			return _transaction.KeyDeleteAsync(key: GetEntityKey(key.ToString()));
		}

		private void EnsureConcurrency(string concurrencyTokenKey, string? expectedConcurrencyToken)
		{
			if (expectedConcurrencyToken is null)
				_transaction.AddCondition(Condition.KeyNotExists(concurrencyTokenKey));
			else
				_transaction.AddCondition(Condition.StringEqual(concurrencyTokenKey, expectedConcurrencyToken));
		}

		private string GetConcurrencyTokenKey(RedisKey key)
			=> $"{Constants.Prefix}.{_moduleName}.{typeof(TEntity).Name}Version#{key}";
	}
}
