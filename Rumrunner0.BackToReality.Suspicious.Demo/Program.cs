using System;
using System.Collections.Generic;
using System.Text.Json;
using Rumrunner0.BackToReality.Suspicious.Monad;

var users = new Dictionary<Guid, string>();
var knownId = Guid.NewGuid();
users[knownId] = "Roman";

// --------------------------------
// 1. Repository miss — the same outcome identity on either rail.

Suspicious<string> FindName(Guid id)
{
	return users.TryGetValue(id, out var name) ? Suspicious.Ok(name) : Suspicious.NoValue<string>();
}

Suspicious<string> LoadRequiredName(Guid id)
{
	return users.TryGetValue(id, out var name)
		? Suspicious.Ok(name)
		: Suspicious.Fail<string>(Error.NoValue($"User {id} is required but missing"));
}

Console.WriteLine("1. Repository miss on either rail");
Console.WriteLine();

var successRail = FindName(Guid.NewGuid());
Console.WriteLine(successRail);
Console.WriteLine($"Is no_value: {successRail.Is(OutcomeKind.NoValue)}; IsSuccess: {successRail.IsSuccess}");

var failureRail = LoadRequiredName(Guid.NewGuid());
Console.WriteLine(failureRail);
Console.WriteLine($"Is no_value: {failureRail.Is(OutcomeKind.NoValue)}; IsSuccess: {failureRail.IsSuccess}");
Console.WriteLine();

// --------------------------------
// 2. Validation — gather all errors via an explicit Combine.

Suspicious ValidateName(string? name)
{
	if (string.IsNullOrWhiteSpace(name)) return Suspicious.Invalid("Name is required");
	return Suspicious.Ok();
}

Suspicious ValidateAge(int age)
{
	if (age is < 0 or > 150) return Suspicious.Invalid($"Age {age} is out of range");
	return Suspicious.Ok();
}

Console.WriteLine("2. Validation aggregate");
Console.WriteLine();

var validation = Suspicious.Combine
(
	ValidateName(null),
	ValidateAge(-5),
	ValidateName("Roman")
);

Console.WriteLine(validation);
Console.WriteLine();

// --------------------------------
// 3. Exception adapter — try/catch at the boundary, results everywhere else.

Suspicious<int> ParseAge(string text)
{
	try
	{
		return Suspicious.Ok(int.Parse(text));
	}
	catch (FormatException e)
	{
		return Suspicious.Unexpected<int>(e, $"'{text}' is not a number");
	}
}

Console.WriteLine("3. Exception adapter");
Console.WriteLine();
Console.WriteLine(ParseAge("42"));
Console.WriteLine(ParseAge("forty-two"));
Console.WriteLine();

// --------------------------------
// 4. Railway pipeline — Then/Map/Match, and the LINQ query form.

Suspicious<int> CountOrders(string name)
{
	return name == "Roman" ? Suspicious.Ok(3) : Suspicious.NoValue<int>();
}

Console.WriteLine("4. Railway pipeline");
Console.WriteLine();

var report = FindName(knownId)
	.Then(CountOrders)
	.Map(static count => $"{count} order(s)")
	.Match
	(
		onValue: static text => $"Report: {text}",
		onNoValue: static () => "Report: nothing to report",
		onError: static e => $"Report failed: {e.Description}"
	);

Console.WriteLine(report);

var query =
	from name in FindName(knownId)
	from count in CountOrders(name)
	select $"{name} has {count} order(s)";

Console.WriteLine(query);
Console.WriteLine();

// --------------------------------
// 5. Unit result — a void-like operation.

Suspicious Delete(Guid id)
{
	if (!users.Remove(id)) return Suspicious.Conflict($"User {id} doesn't exist");
	return Suspicious.Ok();
}

Console.WriteLine("5. Unit result");
Console.WriteLine();
Console.WriteLine(Delete(knownId));
Console.WriteLine(Delete(knownId));
Console.WriteLine();

// --------------------------------
// 6. JSON — the internal transport contract (public APIs should Match into DTOs instead).

Console.WriteLine("6. JSON round-trip");
Console.WriteLine();

var payload = JsonSerializer.Serialize(Suspicious.Fail<string>(Error.Invalid("Name is required", cause: Error.Unavailable("Storage is down"))));
Console.WriteLine(payload);

var restored = JsonSerializer.Deserialize<Suspicious<string>>(payload)!;
Console.WriteLine($"Restored: {restored}");