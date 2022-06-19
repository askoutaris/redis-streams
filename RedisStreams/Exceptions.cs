using System;

namespace RedisStreams
{
	[Serializable]
	public class RedisStreamsException : Exception
	{
		public RedisStreamsException(string message) : base(message) { }
	}

	[Serializable]
	public class TopicNotFoundException : Exception
	{
		public TopicNotFoundException(string message) : base(message) { }
	}

	[Serializable]
	class ConfigurationException : RedisStreamsException
	{
		public ConfigurationException(string message) : base(message) { }
	}
}
