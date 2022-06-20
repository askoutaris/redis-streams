using System;
using System.Collections.Generic;
using System.Linq;

namespace RedisStreams.Routings
{
	class Routing
	{
		public int Version { get; private set; }
		public string ModuleName { get; }
		public IReadOnlyCollection<Topic> Topics { get; private set; }
		public DateTime Updated { get; private set; }
		public string ConcurrencyTokenKey => $"{Constants.Prefix}.{ModuleName}.RoutingVersion";
		public long GenerateVersion() => ++Version;

		public Routing(int version, string moduleName, IReadOnlyCollection<Topic> topics, DateTime updated)
		{
			Version = version;
			ModuleName = moduleName;
			Topics = topics;
			Updated = updated;
		}

		public static Routing CreateNew(string moduleName)
		{
			return new Routing(
				version: 0,
				moduleName: moduleName,
				topics: Array.Empty<Topic>(),
				updated: DateTime.UtcNow);
		}

		public void Update(TopicConfiguration[] configurations)
		{
			Updated = DateTime.UtcNow;

			Topics = GetTopics(Version, configurations);
		}

		private Topic[] GetTopics(int version, TopicConfiguration[] configurations)
		{
			var streams = configurations
				.Select(topic => new Topic(
					name: topic.Name,
					streams: GetStreams(version, topic)))
				.ToArray();

			return streams;
		}

		private Stream[] GetStreams(int version, TopicConfiguration topic)
		{
			return Enumerable.Range(1, topic.PartitionsCount)
				.Select(i => new Stream(
					moduleName: ModuleName,
					topicName: topic.Name,
					partitionNumber: i,
					routingVersion: version))
				.ToArray();
		}
	}
}
