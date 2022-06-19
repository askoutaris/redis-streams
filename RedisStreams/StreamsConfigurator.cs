using System;
using System.Linq;
using System.Threading.Tasks;
using RedisStreams.Routings;
using StackExchange.Redis;

namespace RedisStreams
{
	internal interface IStreamsConfigurator
	{
		Task Configure(IDatabase db);
	}

	public class StreamsConfigurator : IStreamsConfigurator
	{
		private readonly string _moduleName;
		private readonly Topic[] _topics;

		public StreamsConfigurator(string moduleName, Topic[] topics)
		{
			_moduleName = moduleName;
			_topics = topics;
		}

		public async Task Configure(IDatabase db)
		{
			var tran = db.CreateTransaction();

			var routing = await db.RoutingDocuments(_moduleName).TryGet(Constants.Keys.Routing);
			var oldVersion = routing?.Version;

			if (routing is null)
				routing = Routing.CreateNew(moduleName: _moduleName);

			routing.UpdateTopics(_topics);

			foreach (var stream in routing.Streams)
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

			_ = tran.RoutingDocuments(_moduleName).SetAsync(
				key: Constants.Keys.Routing,
				entity: routing,
				expiry: null,
				expectedConcurrencyToken: oldVersion?.ToString(),
				newConcurrencyToken: routing.Version.ToString(),
				concurrencyTokenKey: routing.ConcurrencyTokenKey);

			var completed = await tran.ExecuteAsync();

			if (!completed)
				throw new RedisStreamsException($"Configure transaction failed");
		}
	}
}
