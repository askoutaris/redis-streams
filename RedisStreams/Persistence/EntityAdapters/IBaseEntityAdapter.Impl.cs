using System.Threading.Tasks;
using RedisStreams.Serializers;
using StackExchange.Redis;

namespace RedisStreams.Persistence.EntityAdapters
{
	class BaseEntityAdapter<TEntity> : IBaseEntityAdapter<TEntity> where TEntity : class
	{
		protected readonly string _moduleName;
		protected readonly IRedisSerializer<TEntity> _serializer;

		public BaseEntityAdapter(string moduleName, IRedisSerializer<TEntity> serializer)
		{
			_moduleName = moduleName;
			_serializer = serializer;
		}

		public async Task<bool> KeyExists(IDatabase db, RedisKey key)
		{
			return await db.KeyExistsAsync(GetEntityKey(key.ToString()));
		}

		public async Task<TEntity?> TryGet(IDatabase db, RedisKey key)
		{
			var value = await db.StringGetAsync(key: GetEntityKey(key.ToString()));

			if (value.IsNull)
				return null;

			var entity = _serializer.Deserialize(value);

			return entity;
		}

		protected string GetEntityKey(RedisKey key)
			=> $"{Constants.Prefix}.{_moduleName}.{typeof(TEntity).Name}#{key}";
	}
}
