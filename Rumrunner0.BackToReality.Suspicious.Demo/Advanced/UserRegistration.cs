namespace Rumrunner0.BackToReality.Suspicious.Demo.Advanced;

using System;
using System.Collections.Generic;
using System.Linq;
using Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>A layered registration flow — validation, uniqueness, persistence and the boundary.</summary>
/// <remarks>Results flow through every layer; exceptions stop at the adapter; the boundary folds into a transport shape.</remarks>
internal static class UserRegistration
{
	/// <summary>Registered emails.</summary>
	private static readonly HashSet<string> _emails = new (StringComparer.OrdinalIgnoreCase) { "taken@example.com" };

	/// <summary>Runs the example.</summary>
	internal static void Run()
	{
		Console.WriteLine(ToResponse(Register("Roman", "roman@example.com", "30")));
		Console.WriteLine(ToResponse(Register("", "not-an-email", "abc")));
		Console.WriteLine(ToResponse(Register("Roman", "taken@example.com", "30")));
		Console.WriteLine(ToResponse(Register("Roman", "crash@example.com", "30")));
	}

	/// <summary>Registers a user — validation, uniqueness, persistence.</summary>
	/// <param name="name">The name.</param>
	/// <param name="email">The email.</param>
	/// <param name="ageText">The raw age.</param>
	/// <returns>The registered user, or a failure from any layer.</returns>
	private static Suspicious<User> Register(string name, string email, string ageText)
	{
		// Independent field checks aggregate — the caller gets ALL violations at once.
		var validation = Suspicious.Combine
		(
			string.IsNullOrWhiteSpace(name) ? Suspicious.Invalid("Name is required") : Suspicious.Ok(),
			email.Contains('@') ? Suspicious.Ok() : Suspicious.Invalid($"Email '{email}' is malformed"),
			int.TryParse(ageText, out var age) && age is >= 0 and <= 150 ? Suspicious.Ok() : Suspicious.Invalid($"Age '{ageText}' is out of range")
		);

		if (validation.IsFailure) return Suspicious.Fail<User>(validation.Error);

		// Dependent checks run sequentially and fail fast.
		if (_emails.Contains(email)) return Suspicious.Conflict<User>($"Email '{email}' is already registered");

		return Persist(new User(Guid.NewGuid(), name, email, age));
	}

	/// <summary>Persists a user — the exception adapter: try/catch lives HERE, results everywhere else.</summary>
	/// <param name="user">The user.</param>
	/// <returns>The persisted user, or an <see cref="OutcomeKind.Unexpected" /> failure.</returns>
	private static Suspicious<User> Persist(User user)
	{
		try
		{
			if (user.Email.StartsWith("crash", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Storage rejected the write");

			_emails.Add(user.Email);
			return user;
		}
		catch (InvalidOperationException e)
		{
			return Suspicious.Unexpected<User>(e, "User couldn't be persisted");
		}
	}

	/// <summary>Folds a registration result into an HTTP-ish response line — the boundary.</summary>
	/// <param name="result">The result.</param>
	/// <returns>A response line.</returns>
	private static string ToResponse(Suspicious<User> result)
	{
		// The two-way Match is contractually safe here: no producer in this flow creates a valueless success.
		return result.Match
		(
			onValue: static user => $"201 Created — user {user.Id}",
			onError: static e =>
			{
				if (e.Kind == OutcomeKind.Invalid) return $"400 Bad Request — {Flatten(e)}";
				if (e.Kind == OutcomeKind.Conflict) return $"409 Conflict — {e.Description}";
				if (e.Kind == OutcomeKind.Unavailable) return "503 Service Unavailable";

				return "500 Internal Server Error";
			}
		);
	}

	/// <summary>Flattens an error into one line — aggregate details joined, or the description itself.</summary>
	/// <param name="error">The error.</param>
	/// <returns>A single line.</returns>
	private static string Flatten(Error error)
	{
		return error.Details.Count > 0
			? string.Join("; ", error.Details.Select(static d => d.Description))
			: error.Description ?? error.Kind.Name;
	}
}

/// <summary>A registered user.</summary>
/// <param name="Id">The id.</param>
/// <param name="Name">The name.</param>
/// <param name="Email">The email.</param>
/// <param name="Age">The age.</param>
internal sealed record class User(Guid Id, string Name, string Email, int Age);