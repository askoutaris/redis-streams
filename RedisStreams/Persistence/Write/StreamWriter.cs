using System.Threading.Tasks;
using RedisStreams.Serializers;
using StackExchange.Redis;

namespace RedisStreams.Persistence.Write
{
	interface IStreamWriter<TMessage>
	{
		Task Write(IDatabase db, TMessage message);
		Task Write(IDatabase db, TMessage message, string? messageId);
	}

	class StreamWriter<TMessage> : IStreamWriter<TMessage>
	{
		private readonly string _routingConcurrencyToken;
		private readonly string _routingConcurrencyTokenKey;
		private readonly string _streamKey;
		private readonly int _maxLength;
		private readonly IRedisSerializer<TMessage> _serializer;

		public StreamWriter(string routingConcurrencyToken, string routingConcurrencyTokenKey, string streamKey, int maxLength, IRedisSerializer<TMessage> serializer)
		{
			_routingConcurrencyToken = routingConcurrencyToken;
			_routingConcurrencyTokenKey = routingConcurrencyTokenKey;
			_streamKey = streamKey;
			_maxLength = maxLength;
			_serializer = serializer;
		}

		public Task Write(IDatabase db, TMessage message)
			=> Write(db, message, null);

		public async Task Write(IDatabase db, TMessage message, string? messageId)
		{
			var tran = db.CreateTransaction();

			tran.AddCondition(Condition.StringEqual(_routingConcurrencyTokenKey, _routingConcurrencyToken));

			var body = _serializer.Serialize(message);

			_ = tran.StreamAddAsync(
				key: _streamKey,
				streamField: Constants.StreamFields.Body,
				streamValue: body,
				messageId: messageId,
				maxLength: _maxLength,
				useApproximateMaxLength: true);

			var completed = await tran.ExecuteAsync();

			if (!completed)
				throw new ConfigurationException($"RoutingConcurrency {_routingConcurrencyTokenKey}:{_routingConcurrencyToken} is outdated for StreamWriter: '{_streamKey}' of type {typeof(TMessage).FullName}");
		}
	}
}
