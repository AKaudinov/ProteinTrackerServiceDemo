using System;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceHost;

namespace ProteinTrackerServiceDemo
{
	public class StatusService : Service
	{
		public TrackedDataRepository TrackedDataRepository { get; set; }//will automatically get the only instance of TrackedDataRepository
														//across all application from Funq - if no instance exists, it creates one.
																		

						//accepts any verb url
		public object Any(StatusQuery request) 
		{
			var cacheKey = UrnId.Create<StatusQuery>(request.Date.ToShortDateString()); //unique URN for this request. //everytime the urn comes in, urn will match
																						//so it will know it will be the same exact request each time.
			var log = LogManager.GetLogger(GetType());// creates a logger based on the log factory specified in Global.asax

			log.Info("Made a status request with cacheKey {0}".Fmt(cacheKey)); //Fmt is serviceStack formatting just like String.Format

			return RequestContext.ToOptimizedResultUsingCache(base.Cache, cacheKey, new TimeSpan(0,0,0,25),() =>
			{
				var status = TrackedDataRepository.GetStatus(request.Date, Session, this.GetSession());

				return status;
			}); //handles all the caching for us, use lambda if nothing found in the cache, if something is found in the cache with the cache key
				//it will retrun the data from cache, otherwise lambda is executed and goes against the real data.
				//this cache will live for 25 seconds, and after 25 seconds it will expire.
				//though timing is not required, you can remove invalid entries from the cache.



			//var date = request.Date.Date;
			//var trackedData = (TrackedData)Session[date.ToString()]; //get data from the session if we have it
			//if (trackedData == null)
			//{
			//	trackedData = new TrackedData { Goal = 300, Total = 0 }; //if no data avaialable, create default data
			//}

			//var message = this.GetSession().DisplayName; //session cache is enabled in global.asax.cs

			//return new StatusResponse
			//{
			//	Total = trackedData.Total,
			//	Goal = trackedData.Goal,
			//	Message = message
			//};
		}
	}
}
