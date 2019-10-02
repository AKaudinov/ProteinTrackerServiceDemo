using System;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceInterface;
using ProteinTrackerServiceDemo;
using ServiceStack.ServiceInterface.Auth;

namespace Consumer
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			int amount = -1;
			var client = new JsonServiceClient("http://localhost:64132") {UserName="juser", Password= "password"};
			client.Send<AssignRolesResponse>(new AssignRoles {
				UserName = "juser", 
				Roles = new ArrayOfString("User"), 
				Permissions = new ArrayOfString("GetStatus") }
			//after constructor initialization, set up UserName and Password authentication

			//Soap11ServiceClient
			//Soap12ServiceClient
			//Different clients are availble not only JsonServiceClient

			while (amount != 0) 
			{
				//Call the service and add an entry	
				Console.WriteLine("Enter protein amount (0 to exit):");
				amount = int.Parse(Console.ReadLine());


				//no need to specify the return type here, as the Entry class implements "IReturn" response, and specifies what type gets returned
				//old code //var response = client.Send(new Entry // Send works with any method that is available on the server that accepts calls
				//{
				//	Amount = amount,
				//	Time = DateTime.Now
				//});

				//Async call
				client.SendAsync(new Entry // Send works with any method that is available on the server that accepts calls
				{
					Amount = amount,
					Time = DateTime.Now
				}, entryResponse => Console.WriteLine("Response " + response.Id) // on success
												,(entryResponse, exception) => Console.WriteLine("error"));
				//Service Stack will serialize and desiralize on its own
			}
			//type of response accepted
			//ServiceStack doesn't even need the url anymore, it will figure that out from the Route
			//older code client.Post("status", new StatusQuery{..});
			StatusResponse statusResponse = null;
			try
			{
				statusResponse = client.Post(new StatusQuery //url is needed for POST: http://localhost:64132/status
				{

					Date = DateTime.Now
				});
			}
			catch(WebServiceException exception) 
			{
				Console.WriteLine(exception.ErrorMessage);
				Console.ReadLine();
				return;
			}
			Console.WriteLine("{0} / {1}", statusResponse.Total, statusResponse.Goal);
			Console.WriteLine(statusResponse.Message);
			Console.ReadLine();
			}
			//call the service and get status
		}
	}
}
