using System.Collections.Generic;

namespace RedisStreams.Routings
{
	class Topic
	{
		public string Name { get; }
		public IReadOnlyCollection<Stream> Streams { get; }

		public Topic(string name, Stream[] streams)
		{
			Name = name;
			Streams = streams;
		}
	}
}
