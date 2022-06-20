using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RedisStreams.Persistence.EntityAdapters;
using RedisStreams.Routings;
using RedisStreams.Serializers;
using StackExchange.Redis;

namespace RedisStreams.Persistence.Write
{
	interface IRoutingPersistence
	{
		Task Persist(ITransaction db, Routing routing);
	}

	class RoutingPersistence : IRoutingPersistence
	{
		private readonly IConcurrentEntityAdapter<Routing> _entityAdapter;

		public RoutingPersistence(IConcurrentEntityAdapter<Routing> entityAdapter)
		{
			_entityAdapter = entityAdapter;
		}

		public Task Persist(ITransaction db, Routing routing)
		{
			return _entityAdapter.SetAsync(
				tran: db,
				key: Constants.Keys.Routing,
				entity: routing,
				expiry: null,
				expectedConcurrencyToken: routing.Version == 0 ? null : routing.Version.ToString(), // zero means Routing is new
				newConcurrencyToken: routing.GenerateVersion().ToString(),
				concurrencyTokenKey: routing.ConcurrencyTokenKey);
		}
	}
}
