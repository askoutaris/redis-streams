using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisStreams
{
	interface IMessageBus
	{
		Task Send(IDatabase db, string topicName, string partitionKey, object message);
	}

	class MessageBus : IMessageBus
	{
		private readonly IDictionary<string, ITopicPublisher> _topics;

		public MessageBus(ITopicPublisher[] topics)
		{
			_topics = topics.ToDictionary(x => x.Name);
		}

		public async Task Send(IDatabase db, string topicName, string partitionKey, object message)
		{
			if (!_topics.TryGetValue(topicName, out var writer))
				throw new TopicNotFoundException($"Topic {topicName} not found");

			await writer.Send(db, partitionKey, message);
		}
	}
}
