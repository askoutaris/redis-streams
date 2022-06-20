using System.Collections.Generic;
using System.Threading.Tasks;
using RedisStreams.Persistence.Write;
using StackExchange.Redis;

namespace RedisStreams.Routings
{
	interface ITopicPublisher
	{
		public string Name { get; }
		Task Send(IDatabase db, string partitionKey, object message);
	}

	class TopicPublisher<TMessage> : ITopicPublisher
	{
		private readonly IReadOnlyDictionary<int, IStreamWriter<TMessage>> _streamWriters;

		public string Name { get; }

		public TopicPublisher(string name, IReadOnlyDictionary<int, IStreamWriter<TMessage>> streamWriters)
		{
			Name = name;
			_streamWriters = streamWriters;
		}

		public async Task Send(IDatabase db, string partitionKey, object message)
		{
			var index = GetPartitionIndex(partitionKey);

			await _streamWriters[index].Write(db, (TMessage)message);
		}

		private int GetPartitionIndex(string partitionKey)
			=> partitionKey.GetHashCode() % _streamWriters.Count;
	}
}
