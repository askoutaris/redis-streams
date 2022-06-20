using System.Threading.Tasks;
using RedisStreams.Routings;
using StackExchange.Redis;

namespace RedisStreams.Persistence.Write
{
    interface IRoutingUpdater
	{
		Task UpdateRoutings(ITransaction tran, Routing routing);
	}

	class RoutingUpdater : IRoutingUpdater
	{
		private readonly IRoutingPersistence _routingPersistence;

		public RoutingUpdater(IRoutingPersistence routingPersistence)
		{
			_routingPersistence = routingPersistence;
		}

		public async Task UpdateRoutings(ITransaction tran, Routing routing)
		{
			foreach (var topic in routing.Topics)
			{
				foreach (var stream in topic.Streams)
				{
					_ = tran.StreamAddAsync(
						key: stream.Key,
						streamField: Constants.StreamFields.Body,
						streamValue: Constants.StreamFields.Values.Setup,
						messageId: null,
						maxLength: null);

					// Trim stream to 0 length in order to remove the initial "Setup" message needed to create the streams
					_ = tran.StreamTrimAsync(stream.Key, 0);
				}
			}

			_ = _routingPersistence.Persist(tran, routing);

			var completed = await tran.ExecuteAsync();

			if (!completed)
				throw new RedisStreamsException($"Configure transaction failed");
		}
	}
}
