using System;
using System.Collections.Generic;
using System.Linq;

namespace RedisStreams.Routings
{
	class Routing
	{
		public int Version { get; private set; }
		public string ModuleName { get; }
		public IReadOnlyCollection<Stream> Streams { get; private set; }
		public IReadOnlyCollection<Topic> Topics { get; private set; }
		public DateTime Updated { get; private set; }
		public string ConcurrencyTokenKey => $"{Constants.Prefix}.{ModuleName}.RoutingVersion";

		public Routing(int version, string moduleName, IReadOnlyCollection<Stream> streams, IReadOnlyCollection<Topic> topics, DateTime updated)
		{
			Version = version;
			ModuleName = moduleName;
			Streams = streams;
			Topics = topics;
			Updated = updated;
		}

		public static Routing CreateNew(string moduleName)
		{
			return new Routing(
				version: 0,
				moduleName: moduleName,
				streams: Array.Empty<Stream>(),
				topics: Array.Empty<Topic>(),
				updated: DateTime.UtcNow);
		}

		public void UpdateTopics(Topic[] topics)
		{
			Version++;
			Updated = DateTime.UtcNow;

			Topics = topics;
			Streams = GetStreams(Version, topics);
		}

		private Stream[] GetStreams(int version, Topic[] topics)
		{
			var streams = topics
				.SelectMany(topic => Enumerable.Range(1, topic.PartitionsCount)
					.Select(i => new Stream(
						moduleName: ModuleName,
						topicName: topic.Name,
						partitionNumber: i,
						routingVersion: version)))
				.ToArray();

			return streams;
		}
	}
}
