using System;
using ServiceStack.ServiceHost;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceInterface.ServiceModel;

namespace ProteinTrackerServiceDemo
{
	//[Route("/entry", "POST")] //add serviceStack route = http://localhost:<whatever>/entry
	//[Route("/entry/{Amount}/{Time}", "POST")] //automatically binds Amount and Time parameters to the 'Entry' class
	[Route("/entry")]
	[Route("/entry/{Amount}/{EntryTime}")]
	[RecordIpFilter] //sets up the attribute filter on this specific call

	public class Entry : IReturn<EntryResponse> //what type of response is being sent back
	{
		[AutoIncrement] //automatically increment the Id, ORMLite will serialize thise class
		public int Id { get; set; }
		public DateTime EntryTime { get; set; }
		public int Amount { get; set; }

	}

	public class EntryResponse
	{ 
		public int Id { get; set; }
		public ResponseStatus ResponseStatus { get; set; }
	}

	public class EntryValidator : AbstractValidator<Entry> //this validator will validate Entry class
	{
		public EntryValidator()
		{
			RuleFor(entry => entry.Amount).GreaterThan(0);
			RuleFor(entry => entry.EntryTime).LessThan(DateTime.Now).WithMessage("Date must not be a future date");

			RuleSet(ApplyTo.Get, () => 
			{
				RuleFor(entry => entry.Amount).LessThanOrEqualTo(50); //rule will only apply to a get request.
			});
		}
	}
}
