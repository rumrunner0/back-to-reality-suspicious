using System;
using System.Threading.Tasks;
using Rumrunner0.BackToReality.Suspicious.Demo.Advanced;
using Rumrunner0.BackToReality.Suspicious.Demo.Essentials;

Console.WriteLine("Rumrunner0.BackToReality.Suspicious Guide");

Run("Essentials 1. Creating results", CreatingResults.Run);
Run("Essentials 2. Consuming results", ConsumingResults.Run);
Run("Essentials 3. Outcome kinds", OutcomeKinds.Run);
Run("Essentials 4. A miss on either rail", MissOnEitherRail.Run);
Run("Essentials 5. Chaining", ChainingResults.Run);
Run("Essentials 6. Query syntax", QuerySyntax.Run);
Run("Essentials 7. Combining", CombiningResults.Run);
Run("Essentials 8. Errors and custom kinds", ErrorsAndCustomKinds.Run);

Run("Advanced 1. User registration", UserRegistration.Run);
Run("Advanced 2. Partial import", PartialImport.Run);
Run("Advanced 3. Error triage", ErrorTriage.Run);
Run("Advanced 4. JSON transport", JsonTransport.Run);
Run("Advanced 5. Order checkout", OrderCheckout.Run);
await RunAsync("Advanced 6. Async pipeline", AsyncPipeline.Run);

return;

static void Run(string title, Action example)
{
	Console.WriteLine();
	Console.WriteLine($"--- {title} ---");
	Console.WriteLine();
	example();
}

static async Task RunAsync(string title, Func<Task> example)
{
	Console.WriteLine();
	Console.WriteLine($"--- {title} ---");
	Console.WriteLine();
	await example();
}