namespace RedisStreams.Routings
{
	class Stream
	{
		public string Key => $"{Constants.Prefix}.{ModuleName}.{Constants.Streams.Prefix}.{TopicName}.V{RoutingVersion}.{PartitionNumber}";

		public string ModuleName { get; }
		public string TopicName { get; }
		public int PartitionNumber { get; }
		public int RoutingVersion { get; }

		public Stream(string moduleName, string topicName, int partitionNumber, int routingVersion)
		{
			ModuleName = moduleName;
			TopicName = topicName;
			PartitionNumber = partitionNumber;
			RoutingVersion = routingVersion;
		}
	}
}
