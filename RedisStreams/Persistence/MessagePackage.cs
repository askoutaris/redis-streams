namespace RedisStreams.Persistence
{
	struct MessagePackage<TMessage>
	{
		public string Id { get; }
		public TMessage Message { get; }

		public MessagePackage(string id, TMessage message)
		{
			Id = id;
			Message = message;
		}
	}
}
