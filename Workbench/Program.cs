using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RedisStreams;
using RedisStreams.Routings;
using StackExchange.Redis;

namespace Workbench
{
	class Program
	{
		private const string _streamKey = "mystream";

		static async Task Main(string[] args)
		{
			var multiplexer = ConnectionMultiplexer.Connect("redis.dev-sb-docker.local,allowAdmin=true,defaultDatabase=0");

			// setup our DI
			var serviceProvider = new ServiceCollection()
				.AddTransient<IDatabase>(_ => multiplexer.GetDatabase())
				.BuildServiceProvider();

			var streamConfigurator = StreamsManager.CreateNew("SportsbookPlayers", multiplexer);

			var topics = new[] {
				new  TopicConfiguration("MarketOfferMessages", 5)
			};

			await streamConfigurator.Configure(topics);

			//await StreamAdd(db);
			//await StreamCreateConsumer(db);
			//await StreamRead(db);
			//await StreamClaim(db);

			Console.ReadLine();
		}

		private static async Task StreamRead(IDatabase db)
		{
			var tran = db.CreateTransaction();

			tran.AddCondition(Condition.KeyExists("Version3"));

			var pendingValuesTask = tran.StreamReadGroupAsync(_streamKey, _streamKey, _streamKey, "0", 10);
			var newValuesTask = tran.StreamReadGroupAsync(_streamKey, _streamKey, _streamKey, ">", 10);


			var completed = await tran.ExecuteAsync();

			if (completed)
			{
				var pendingValues = await pendingValuesTask;
				var newValues = await newValuesTask;
			}
		}

		private static async Task StreamClaim(IDatabase db)
		{
			ITransaction tran = db.CreateTransaction();

			tran.AddCondition(Condition.KeyExists("Version4"));

			_ = tran.StreamAutoClaimAsync(_streamKey, _streamKey, _streamKey + "2", 0, 0);

			var completed = await tran.ExecuteAsync();
		}

		private static async Task StreamCreateConsumer(IDatabase db)
		{
			var tran = db.CreateTransaction();

			tran.AddCondition(Condition.KeyExists("Version2"));

			tran.StreamCreateConsumerGroupAsync(_streamKey, _streamKey, "0");

			var completed = await tran.ExecuteAsync();
		}

		private static async Task StreamAdd(IDatabase db)
		{
			var tran = db.CreateTransaction();

			tran.AddCondition(Condition.KeyExists("Version1"));

			tran.StreamAddAsync(key: _streamKey, "body", $"{DateTime.Now:o}");

			var completed = await tran.ExecuteAsync();
		}
	}
}
