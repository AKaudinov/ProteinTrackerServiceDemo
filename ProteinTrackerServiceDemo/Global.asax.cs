using System.Web;
using ServiceStack.WebHost.Endpoints;
using Ninject;
                  
namespace ProteinTrackerServiceDemo
{
	public class Global : HttpApplication
	{
		public class ProteinTrackerAppHost : AppHostBase
		{
			public ProteinTrackerAppHost() : base("Protein Tracker", typeof(EntryService).Assembly) { }

			public override void Configure(Funq.Container container)
			{
				//Confiture our application

				//IKernel kernel = new StandardKernel();
				//kernel.Bind<TrackedDataRepository>().ToSelf(); //similar to funq, singleton. Class resolved to itself

				//container.Adapter = new NinjectContainerAdapter(kernel);


				//Authentication configuration - iOC container
				Plugins.Add(new AuthFeature(() => new AuthUserSession(),
											new IAuthProvider[]
				{ new BasicAuthProvider(),
					new TwitterAuthProvider(new AppSettings())

				}));


				Plugins.Add(new RegistrationFeature()); //registration for new user

				Plugins.Add(new ValidationFeature());
				container.RegisterValidators(typeof(EntryService).Assembly);//configure validators


				Plugins.Add(new RequestLogsFeature()); //enable request logs feature, a user with Admin privilages is needed

				Plugins.Add(new RazorFormat());

				container.Register<ICacheClient>(new MemoryCacheClient()); //cache client // can set up Redis/Azure

				//Redis container.Register<IRedisClientManager>(c => new PooledRedisClientManager(redis client path));
				//container.Register<ICacheClient>(c =>(ICacheClient)c.Resolve<IRedisClientManager>().GetCacheClient(); < resolve to cache client Redis' client.

				var userRepository = new InMemoryAuthRepository(); //in memory auth repository
				container.Register<IUserAuthRepository>(userRepository);

				//add a user
				string hash;
				string salt;

				new SaltedHash().GetHashAndSaltString("password", out hash, out salt);
				userRepository.CreateUserAuth(new UserAuth
				{
					Id = 1,
					DisplayName = "JoeUser",
					Email = "joe@user.com",
					UserName = "juser",
					FirstName = "Joe",
					LastName = "User",
					PasswordHash = hash,
					salt = salt,
					Roles = new List<string> { RoleNames.Admin } // have to have this role in order to call AssignRoles, UnAssignRoles default services
																 //Roles = new List<string> { "User" },//this is Role Authorization
																 //Permissions = new List<string> {"GetStatus"} //this is Permission Authorization
				}, "password"); //user is created in the repository

				var dbConnectionFactory = new OrmLiteConnectionFactory(HttpContext.Current.Server.MapPath("~/App_Data/data.txtx"),
																	   SqlliteDialect.Provider)
				{
					ConnectionFilter = x => new ProfiledDbConnection(x, Profiler.Current); //enables any calls through ORM to be profiled.
			};


			container.Register<IDbConnectionFactory>(dbConnectionFactory) //anywhere you want to use dbConnectionFactory, Funq will automatically populate this

				container.RegisterAutoWired<TrackedDataRepository>(); //Autowiring services and auto inject any public properties, SINGLETON by default
																	  //You can set the scope with ReusedWIthin

				LogManager.LogFactory = new EventLogFactory("ProteinTracker.Logging", "Application");
			                                                //name					//source

				SetConfig(new EndpointHostConfig {DebugMode = true});

				container.Register<IRedisClientsManager>(new PooledRedisClientManager("localhost:6379"));//default redis port
				var mqService = new RedisMqServer(container.Resolve<IRedisClientsManager>());


				mqService.RegisterHandler<Entry>(ServiceController.ExecuteMessage); //executs the service that handles the Entry message
				//listens for Entry messages

				mqSerice.Start(); //start listening for messages
			}
		}
		protected void Application_Start()
		{
			new ProteinTrackerAppHost().Init();
		}


		//Profiling configuration
		protected void Application_BeginRequest(object sender, EventArgs e)
		{
			if (Request.IsLocal)
			{
				Profiler.Start();
			}
		}

		protected void Application_EndRequest(object sender, EventArgs e)
		{
			Profiler.Stop();
		}
	}
}
