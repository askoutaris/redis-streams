using System.Threading.Tasks;
using RedisStreams.Serializers;
using StackExchange.Redis;

namespace RedisStreams.Persistence.EntityAdapters
{
	class BaseEntityAdapter<TEntity> : IBaseEntityAdapter<TEntity> where TEntity : class
	{
		private readonly IDatabaseAsync _db;
		protected readonly string _moduleName;
		protected readonly IRedisSerializer<TEntity> _serializer;

		public BaseEntityAdapter(string moduleName, IDatabaseAsync db, IRedisSerializer<TEntity> serializer)
		{
			_moduleName = moduleName;
			_db = db;
			_serializer = serializer;
		}

		public async Task<bool> KeyExists(RedisKey key)
		{
			return await _db.KeyExistsAsync(GetEntityKey(key.ToString()));
		}

		public async Task<TEntity?> TryGet(RedisKey key)
		{
			var value = await _db.StringGetAsync(key: GetEntityKey(key.ToString()));

			if (value.IsNull)
				return null;

			var entity = _serializer.Deserialize(value);

			return entity;
		}

		protected string GetEntityKey(RedisKey key)
			=> $"{Constants.Prefix}.{_moduleName}.{typeof(TEntity).Name}#{key}";
	}
}
