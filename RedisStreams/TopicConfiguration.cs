namespace RedisStreams
{
	public class TopicConfiguration
	{
		public string Name { get; }
		public int PartitionsCount { get; }

		public TopicConfiguration(string name, int partitionsCount)
		{
			Name = name;
			PartitionsCount = partitionsCount;
		}
	}
}
