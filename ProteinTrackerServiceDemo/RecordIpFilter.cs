using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.CacheAccess;

namespace ProteinTrackerServiceDemo
{
	public class RecordIpFilter : RequestFilterAttribute //provides basic functionality
	{

		public ICacheClient Cache { get; set; } //automatically gets injected by ServiceStack via dependency injection

		public override void Execute(IHttpRequest req, IHttpResponse res, object requestDto)
		{
			//look at the request, and store it in the cache that has been setup in global.asax
			Cache.Add("last ip", req.UserHostAddress);
		}
	}

	public class LastIpFilter : ResponseFilterAttribute
	{ 
		public ICacheClient Cache { get; set; }
		public override void Execute(IHttpRequest req, IHttpResponse res, object responseDto)
		{
			var status = responseDto as StatusResponse; //apply filter to StatusReponse class, responseDto as StatusReponse type
			if (status != null)
			{
				status.Message += "Last IP: " + Cache.Get<string>("last ip");
			}
		}
	}
}
