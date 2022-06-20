using System;
using System.Threading.Tasks;
using RedisStreams.Serializers;
using StackExchange.Redis;

namespace RedisStreams.Persistence.EntityAdapters
{
	class EntityAdapter<TEntity> : BaseEntityAdapter<TEntity>, IEntityAdapter<TEntity> where TEntity : class
	{
		public EntityAdapter(string moduleName, IRedisSerializer<TEntity> serializer) : base(moduleName, serializer)
		{
		}

		public bool Set(IDatabase db, RedisKey key, TEntity entity, TimeSpan? expiry)
		{
			return db.StringSet(
				key: GetEntityKey(key),
				value: _serializer.Serialize(entity),
				expiry: expiry);
		}

		public Task<bool> SetAsync(IDatabase db, RedisKey key, TEntity entity, TimeSpan? expiry)
		{
			return db.StringSetAsync(
				key: GetEntityKey(key),
				value: _serializer.Serialize(entity),
				expiry: expiry);
		}

		public bool Remove(IDatabase db, RedisKey key)
		{
			return db.KeyDelete(key: GetEntityKey(key.ToString()));
		}

		public Task<bool> RemoveAsync(IDatabase db, RedisKey key)
		{
			return db.KeyDeleteAsync(key: GetEntityKey(key.ToString()));
		}
	}
}
