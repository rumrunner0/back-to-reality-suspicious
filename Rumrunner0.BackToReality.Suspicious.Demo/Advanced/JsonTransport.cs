namespace Rumrunner0.BackToReality.Suspicious.Demo.Advanced;

using System;
using System.Text.Json;
using Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>JSON — the internal transport contract and the boundary rule.</summary>
/// <remarks>The converters exist for internal transport, persistence and logging; public APIs should fold into DTOs instead.</remarks>
internal static class JsonTransport
{
	/// <summary>Runs the example.</summary>
	internal static void Run()
	{
		// Results serialize with attribute-wired converters — no setup required.
		var failure = Suspicious.Fail<string>(Error.Invalid("Name is required", cause: Error.Unavailable("Storage is down")));
		var payload = JsonSerializer.Serialize(failure);

		Console.WriteLine(payload);
		Console.WriteLine($"Restored: {JsonSerializer.Deserialize<Suspicious<string>>(payload)}");

		// Exceptions round-trip lossily BY DESIGN: {type, message} out, null back in.
		Console.WriteLine(JsonSerializer.Serialize(Error.Unexpected(new TimeoutException("No response in 30 s"))));

		// The boundary rule: public API schemas shouldn't expose results — fold into a DTO.
		var dto = failure.Match
		(
			onValue: static v => new ResponseDto("ok", v, null),
			onNoValue: static () => new ResponseDto("empty", null, null),
			onError: static e => new ResponseDto("error", null, e.Description)
		);

		Console.WriteLine(JsonSerializer.Serialize(dto));
	}
}

/// <summary>A public-facing transport shape.</summary>
/// <param name="Status">The status.</param>
/// <param name="Value">The value, if any.</param>
/// <param name="Message">The message, if any.</param>
internal sealed record class ResponseDto(string Status, string? Value, string? Message);