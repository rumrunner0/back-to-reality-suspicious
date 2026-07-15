namespace Rumrunner0.BackToReality.Suspicious.Demo.Essentials;

using System;
using Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>LINQ query syntax — C#'s do-notation over the monad.</summary>
internal static class QuerySyntax
{
	/// <summary>Runs the example.</summary>
	internal static void Run()
	{
		// Each `from` is a Then; the select is a Map. Reads like the happy path, short-circuits like a pipeline.
		var report =
			from name in FindUser("roman")
			from orders in CountOrders(name)
			select $"{name} has {orders} order(s)";

		Console.WriteLine(report);

		// The first miss (or failure) aborts the rest of the query.
		var aborted =
			from name in FindUser("ghost")
			from orders in CountOrders(name)
			select $"{name} has {orders} order(s)";

		Console.WriteLine(aborted);
	}

	/// <summary>Finds a user by handle.</summary>
	/// <param name="handle">The handle.</param>
	/// <returns>The user name, or a successful miss.</returns>
	private static Suspicious<string> FindUser(string handle)
	{
		return handle == "roman" ? Suspicious.Ok("Roman") : Suspicious.NoValue<string>();
	}

	/// <summary>Counts the orders of a user.</summary>
	/// <param name="name">The user name.</param>
	/// <returns>The order count.</returns>
	private static Suspicious<int> CountOrders(string name)
	{
		return name.Length > 0 ? Suspicious.Ok(3) : Error.Invalid("Name is empty");
	}
}