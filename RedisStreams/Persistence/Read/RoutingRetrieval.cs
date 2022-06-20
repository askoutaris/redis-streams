using System.Threading.Tasks;
using RedisStreams.Persistence.EntityAdapters;
using RedisStreams.Routings;
using StackExchange.Redis;

namespace RedisStreams.Persistence.Read
{
	interface IRoutingRetrieval
	{
		Task<Routing?> TryRetrieve(IDatabase db);
	}

	class RoutingRetrieval : IRoutingRetrieval
	{
		private readonly IEntityAdapter<Routing> _entityAdapter;

		public RoutingRetrieval(IEntityAdapter<Routing> entityAdapter)
		{
			_entityAdapter = entityAdapter;
		}

		public Task<Routing?> TryRetrieve(IDatabase db)
		{
			return _entityAdapter.TryGet(db, Constants.Keys.Routing);}
	}
}
