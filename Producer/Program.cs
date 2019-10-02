using System;
using ServiceStack.WebHost.Endpoints;
using ProteinTrackerServiceDemo;

namespace Producer
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Starting producer");
			var clientAppHost = new ClientAppHost();
			clientAppHost.Init();
			clientAppHost.Start("http://localhost:81/");
			while (true)
			{
				Console.WriteLine("Press Enter to send a message");
				Console.ReadLine();
				clientAppHost.SendMessage();
			}
		}
	}

	public class ClientAppHost : AppHostHttpListenerBase
	{
		private RedisMqHost mqHost;

		public ClientAppHost() : base("Producer Console App", typeof(Entry).Assembly) //self serve, without iis
		{ 
			
		}

		public override void Configure(Container container)
		{
			var redisFactory = new PooledRedisClientManager("localhost:6379");
			 mqHost = new RedisMqHost(redisFactory);

			mqHost.RegisterHandler<EntryResponse>(message => {
				Console.WriteLine("Got message id: {0}", message.GetBody().Id);
				return null;
			});


			mqHost.Start();
		}

		public void SendMessage()
		{
			using (var mqClient = mqHost.CreateMessageQueueClient())
			{
				var uniqueQ = "mq:c1" + ":" + Guid.NewGuid().ToString("N"); //unique queue name
				var message = 
					new Message<Entry>(new Entry { Amount = 24, EntryTime = DateTime.Now })
					{ 
						ReplyTo = uniqueQ //reply to this specific queue
					}

				mqClient.Publish(message);

				var response mqClient.Get(uniqueQ, new TimeSpan(0, 0, 1, 0)).ToMessage<EntryResponse>();
										//Wait for the response to come back. Get message response of type Entry Response.

				Console.WriteLine("Got response with id {0}", response.GetBody().Id); 

				//this method will listen on this queue, the defined RegisterHandler above in Configure method will not pick up the response	
				//because it won't be listening on the same channel, the handler in the configure method is running on a general channel,
				//not on a unique queue that has been created.
			}
		}
	}
}
