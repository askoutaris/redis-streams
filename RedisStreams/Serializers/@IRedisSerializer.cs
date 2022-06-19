using StackExchange.Redis;

namespace RedisStreams.Serializers
{
	public interface IRedisSerializer<TType>
	{
		TType Deserialize(RedisValue value);
		RedisValue Serialize(TType obj);
	}
}
