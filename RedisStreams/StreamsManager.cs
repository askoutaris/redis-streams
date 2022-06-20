using System.Threading.Tasks;
using RedisStreams.Persistence.EntityAdapters;
using RedisStreams.Persistence.Read;
using RedisStreams.Persistence.Write;
using RedisStreams.Routings;
using RedisStreams.Serializers;
using StackExchange.Redis;

namespace RedisStreams
{
    public class StreamsManager
	{
		private readonly string _moduleName;
		private readonly IConnectionMultiplexer _multiplexer;
		private readonly IRoutingRetrieval _routingRetrieval;
		private readonly IRoutingUpdater _routingUpdater;

		public static StreamsManager CreateNew(string moduleName, IConnectionMultiplexer multiplexer)
		{
			var routingSerilizer = new RedisNewtonsoftSerializer<Routing>();
			var routingAdapter = new EntityAdapter<Routing>(moduleName, routingSerilizer);
			var routingRetrieval = new RoutingRetrieval(routingAdapter);

			var routingConcurrentAdapter = new ConcurrentEntityAdapter<Routing>(moduleName, routingSerilizer);
			var routingPersistence = new RoutingPersistence(routingConcurrentAdapter);
			var routingUpdater = new RoutingUpdater(routingPersistence);

			return new StreamsManager(
				moduleName: moduleName,
				multiplexer: multiplexer,
				routingRetrieval: routingRetrieval,
				routingUpdater: routingUpdater);
		}

		internal StreamsManager(string moduleName, IConnectionMultiplexer multiplexer, IRoutingRetrieval routingRetrieval, IRoutingUpdater routingUpdater)
		{
			_multiplexer = multiplexer;
			_routingRetrieval = routingRetrieval;
			_moduleName = moduleName;
			_routingUpdater = routingUpdater;
		}

		public async Task Configure(TopicConfiguration[] configurations)
		{
			var db = _multiplexer.GetDatabase();

			var routing = await GetOrCreateRouting(db);

			routing.Update(configurations);

			await PersistRouting(db, routing);
		}

		private async Task<Routing> GetOrCreateRouting(IDatabase db)
		{
			var routing = await _routingRetrieval.TryRetrieve(db);

			return routing ?? Routing.CreateNew(_moduleName);
		}

		private async Task PersistRouting(IDatabase db, Routing routing)
		{
			var tran = db.CreateTransaction();

			await _routingUpdater.UpdateRoutings(tran, routing);
		}
	}
}
