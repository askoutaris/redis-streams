namespace RedisStreams.Routings
{
	public class Topic
	{
		public string Name { get; }
		public int PartitionsCount { get; }

		public Topic(string name, int partitionsCount)
		{
			Name = name;
			PartitionsCount = partitionsCount;
		}
	}
}
