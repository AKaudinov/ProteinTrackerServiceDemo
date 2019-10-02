using System;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceHost;

namespace ProteinTrackerServiceDemo
{
	public class EntryService : Service
	{

		public TrackedDataRepository TrackedDataRepository {get;set;} //will automatically get the only instance of TrackedDataRepository
			//from Funq - if no instance exists, it creates one.

		public object Any(Entry request) //method post, has entry for request
		{

			var cacheKey = urnId.Create<StatusQuery>(request.EntryTime.ToShortDateString()); //want to invalidate status query data, if new data is entered for the same date

			base.RequestContext.RemoveFromCache(base.Cache, cacheKey); 
			//remove from cache, if this new request matches the old cached data in Status Call.


			var id = TrackedDataRepository.AddEntry(request.Amount, request.EntryTime, Session);

			return new EntryResponse { Id = id };

			//var date = request.Time.Date;

			//var trackedData = (TrackedData)Session[date.ToString()]; //using sessions, data is being used as the key

			//if (trackedData == null) 
			//{
			//	trackedData = new TrackedData { Goal = 300 };
			//}


			//trackedData.Total += request.Amount;
			//Session[date.ToString()] = trackedData; //store data back into session key

			//return new EntryResponse 
			//{ 
			//	Id = 1 
			//}; //service stack returns the EntryRespone, smart enough to do everything for you
		}
	}

	public class TrackedData
	{ 
		public int Total { get; set; }
		public int Goal { get; set; }
	}

	public class TrackedDataRepository
	{

		public IDbConnectionFactory DbConnectionFactory { get; set; } //funq automatically wires up orm lite
		public int AddEntry(Entry entry)
		{
			entry.EntryTime = entry.EntryTime.Date;
			using (var db = DbConnectionFactory.OpenDbConnection())
			{
				db.CreateTable<Entry>(); //creates the entry table if it doesn't exist, adds everything on its own, no extra configuration
				db.Insert(entry);
				return (int) db.GetLastInsertId();
			}
			//var date = time.Date;

			//var trackedData = (TrackedData)session[date.ToString()];
			//if (trackedData == null)
			//{
			//	trackedData = new TrackedData { Goal = 300 };
			//}

			//trackedData.Total += amount;
			//session[date.ToString()] = trackedData;
		}

		public StatusResponse GetStatus(DateTime fullDate, ISession session, IAuthSession authSession)
		{
			var message = authSession.DisplayName;

			using (var db = DbConnectionFactory.OpenDbConnection())
			{
				var total = db.Select<Entry>(e => e.EntryTime == fullDate.Date).Sum(e => e.Amount);//can use sql query here
				return new StatusResponse { Goal = 300, Message = message, Total = total };
			}

			//return new StatusResponse { };
			//var date = fullDate.Date;
			//var trackedData = (TrackedData)session[date.ToString()];
			//if (trackedData == null)
			//	trackedData = new TrackedData { Goal = 300, Total = 0 };

			//var message = authSession.DisplayName;

			//return new StatusResponse { Total = trackedData.Total, Goal = trackedData.Goal, Message = message };
		}
	
	}
}
