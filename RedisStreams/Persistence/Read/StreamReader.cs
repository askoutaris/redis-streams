using System.Collections.Generic;
using System.Threading.Tasks;
using RedisStreams.Serializers;
using StackExchange.Redis;

namespace RedisStreams.Persistence.Read
{
	interface IStreamReader<TMessage>
	{
		Task<MessagePackage<TMessage>[]> GetMessages(IDatabase db);
	}

	class StreamReader<TMessage> : IStreamReader<TMessage>
	{
		private readonly string _routingConcurrencyToken;
		private readonly string _routingConcurrencyTokenKey;
		private readonly string _streamKey;
		private readonly string _groupName;
		private readonly string _consumerName;
		private readonly int _batchSize;
		private readonly IRedisSerializer<TMessage> _serializer;

		public StreamReader(string routingConcurrencyToken,string routingConcurrencyTokenKey, string streamKey, string groupName, string consumerName, int batchSize, IRedisSerializer<TMessage> serializer)
		{
			_routingConcurrencyToken = routingConcurrencyToken;
			_routingConcurrencyTokenKey = routingConcurrencyTokenKey;
			_streamKey = streamKey;
			_groupName = groupName;
			_consumerName = consumerName;
			_batchSize = batchSize;
			_serializer = serializer;
		}

		public async Task<MessagePackage<TMessage>[]> GetMessages(IDatabase db)
		{
			var tran = db.CreateTransaction();

			tran.AddCondition(Condition.StringEqual(_routingConcurrencyTokenKey, _routingConcurrencyToken));

			var pendingValuesTask = tran.StreamReadGroupAsync(_streamKey, _groupName, _consumerName, "0", _batchSize);
			var newValuesTask = tran.StreamReadGroupAsync(_streamKey, _groupName, _consumerName, ">", _batchSize);

			var succeeded = await tran.ExecuteAsync();

			if (!succeeded)
				throw new ConfigurationException($"RoutingConcurrency {_routingConcurrencyTokenKey}:{_routingConcurrencyToken} is outdated for StreamReader: '{_streamKey}-{_groupName}-{_consumerName}' of type {typeof(TMessage).FullName}");

			var pendingEntries = await pendingValuesTask;
			var newEntries = await newValuesTask;

			var messages = new List<MessagePackage<TMessage>>(pendingEntries.Length + newEntries.Length);
			foreach (var entry in pendingEntries)
				messages.Add(GetMessagePackage(entry));
			foreach (var entry in newEntries)
				messages.Add(GetMessagePackage(entry));

			return messages.ToArray();
		}

		private MessagePackage<TMessage> GetMessagePackage(StreamEntry entry)
		{
			var field = entry.Values[0];
			var message = _serializer.Deserialize(field.Value);
			var package = new MessagePackage<TMessage>(entry.Id.ToString(), message);
			return package;
		}
	}
}
