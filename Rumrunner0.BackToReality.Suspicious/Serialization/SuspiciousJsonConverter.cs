namespace Rumrunner0.BackToReality.Suspicious.Serialization;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>JSON converter for the unit <see cref="Suspicious" />.</summary>
/// <remarks>This JSON is meant for internal transport, persistence and logging — public APIs should <c>Match</c> into DTOs instead of serializing results.</remarks>
public sealed class SuspiciousJsonConverter : JsonConverter<Suspicious>
{
	/// <summary>JSON property name of the outcome.</summary>
	private const string _outcomePropertyName = "outcome";

	/// <summary>JSON property name of the error.</summary>
	private const string _errorPropertyName = "error";

	/// <inheritdoc />
	public override Suspicious Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using var document = JsonDocument.ParseValue(ref reader);
		var root = document.RootElement;

		if (root.ValueKind != JsonValueKind.Object) throw new JsonException($"A {nameof(Suspicious)} must be a JSON object");

		if (!root.TryGetProperty(_outcomePropertyName, out var outcomeElement)) throw new JsonException($"A {nameof(Suspicious)} requires an '{_outcomePropertyName}'");
		var outcome = outcomeElement.Deserialize<OutcomeKind>(options);
		if (outcome is null) throw new JsonException($"A {nameof(Suspicious)} requires a non-null '{_outcomePropertyName}'");

		var error = root.TryGetProperty(_errorPropertyName, out var errorElement) && errorElement.ValueKind == JsonValueKind.Object ? errorElement.Deserialize<Error>(options) : null;

		if (error is not null)
		{
			if (error.Kind != outcome) throw new JsonException($"The '{_outcomePropertyName}' ({outcome}) doesn't match the '{_errorPropertyName}' kind ({error.Kind})");
			return Suspicious.Fail(error);
		}

		try
		{
			return Suspicious.Success(outcome);
		}
		catch (ArgumentException exception)
		{
			throw new JsonException($"The outcome '{outcome}' requires an '{_errorPropertyName}'", exception);
		}
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, Suspicious value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WritePropertyName(_outcomePropertyName);
		JsonSerializer.Serialize(writer, value.Outcome, options);

		if (value.Error is not null)
		{
			writer.WritePropertyName(_errorPropertyName);
			JsonSerializer.Serialize(writer, value.Error, options);
		}

		writer.WriteEndObject();
	}
}