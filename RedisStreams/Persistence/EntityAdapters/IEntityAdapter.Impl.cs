using System;
using System.Threading.Tasks;
using RedisStreams.Serializers;
using StackExchange.Redis;

namespace RedisStreams.Persistence.EntityAdapters
{
	class EntityAdapter<TEntity> : BaseEntityAdapter<TEntity>, IEntityAdapter<TEntity> where TEntity : class
	{
		private readonly IDatabase _db;

		public EntityAdapter(string moduleName, IDatabase db, IRedisSerializer<TEntity> serializer) : base(moduleName, db, serializer)
		{
			_db = db;
		}

		public bool Set(RedisKey key, TEntity entity, TimeSpan? expiry)
		{
			return _db.StringSet(
				key: GetEntityKey(key),
				value: _serializer.Serialize(entity),
				expiry: expiry);
		}

		public Task<bool> SetAsync(RedisKey key, TEntity entity, TimeSpan? expiry)
		{
			return _db.StringSetAsync(
				key: GetEntityKey(key),
				value: _serializer.Serialize(entity),
				expiry: expiry);
		}

		public bool Remove(RedisKey key)
		{
			return _db.KeyDelete(key: GetEntityKey(key.ToString()));
		}

		public Task<bool> RemoveAsync(RedisKey key)
		{
			return _db.KeyDeleteAsync(key: GetEntityKey(key.ToString()));
		}
	}
}
