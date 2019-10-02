using System;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.ServiceModel;
using ServiceStack.ServiceInterface.Auth;

namespace ProteinTrackerServiceDemo
{
	[Route("/status")]
	//[Authenticate] //the user needs to be logged in, can be applied to a method, class, or a service.
	//[RequiredRole("User")] //cann apply to specific verbs like POST or GET
	//[RequiredPermission("GetStatus")] //permission
		[Route("/status/{Date}")] //if you have a client, you can take this off, as ASP.NET might think this is a dangerous url

	public class StatusQuery : IReturn<StatusResponse>
	{
		public DateTime Date { get; set; }
	}

	[LastIpFilter(ApplyTo = ApplyTo.Patch] //calls the filter before StatusResponse is returned, only applies this filter to a patch
	public class StatusResponse
	{ 
		public int Total { get; set; }
		public int Goal { get; set; }
		public string Message { get; set; }
		public ResponseStatus ResponseStatus { get; set; } //automatically gets populated by servicestack 
		//with response status such as errors, your client can catch those exceptions.
	}
}
