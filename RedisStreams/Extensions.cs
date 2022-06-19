using RedisStreams.Persistence.EntityAdapters;
using RedisStreams.Routings;
using RedisStreams.Serializers;
using StackExchange.Redis;

namespace RedisStreams
{
	static class IDatabaseExtensions
	{
		public static IEntityAdapter<Routing> RoutingDocuments(this IDatabase db, string moduleName)
		{
			var serializer = new RedisNewtonsoftSerializer<Routing>();
			var adapter = new EntityAdapter<Routing>(moduleName, db, serializer);
			return adapter;
		}

		public static IConcurrentEntityAdapter<Routing> RoutingDocuments(this ITransaction db, string moduleName)
		{
			var serializer = new RedisNewtonsoftSerializer<Routing>();
			var adapter = new ConcurrentEntityAdapter<Routing>(moduleName, db, serializer);
			return adapter;
		}
	}
}
