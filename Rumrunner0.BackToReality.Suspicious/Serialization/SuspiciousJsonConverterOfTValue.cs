namespace Rumrunner0.BackToReality.Suspicious.Serialization;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>JSON converter for <see cref="Suspicious{TValue}" />.</summary>
/// <typeparam name="TValue">The value type.</typeparam>
/// <remarks>This JSON is meant for internal transport, persistence and logging — public APIs should <c>Match</c> into DTOs instead of serializing results.</remarks>
public sealed class SuspiciousJsonConverterOfTValue<TValue> : JsonConverter<Suspicious<TValue>> where TValue : notnull
{
	/// <summary>JSON property name of the outcome.</summary>
	private const string _outcomePropertyName = "outcome";

	/// <summary>JSON property name of the value.</summary>
	private const string _valuePropertyName = "value";

	/// <summary>JSON property name of the error.</summary>
	private const string _errorPropertyName = "error";

	/// <inheritdoc />
	public override Suspicious<TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using var document = JsonDocument.ParseValue(ref reader);
		var root = document.RootElement;

		if (root.ValueKind != JsonValueKind.Object) throw new JsonException($"A {nameof(Suspicious<TValue>)} must be a JSON object");

		if (!root.TryGetProperty(_outcomePropertyName, out var outcomeElement)) throw new JsonException($"A {nameof(Suspicious<TValue>)} requires an '{_outcomePropertyName}'");
		var outcome = outcomeElement.Deserialize<OutcomeKind>(options);
		if (outcome is null) throw new JsonException($"A {nameof(Suspicious<TValue>)} requires a non-null '{_outcomePropertyName}'");

		var hasValue = root.TryGetProperty(_valuePropertyName, out var valueElement);
		var error = root.TryGetProperty(_errorPropertyName, out var errorElement) && errorElement.ValueKind == JsonValueKind.Object ? errorElement.Deserialize<Error>(options) : null;

		if (hasValue && error is not null) throw new JsonException($"A {nameof(Suspicious<TValue>)} can't have both a '{_valuePropertyName}' and an '{_errorPropertyName}'");

		if (error is not null)
		{
			if (error.Kind != outcome) throw new JsonException($"The '{_outcomePropertyName}' ({outcome}) doesn't match the '{_errorPropertyName}' kind ({error.Kind})");
			return Suspicious.Fail<TValue>(error);
		}

		try
		{
			if (hasValue)
			{
				var value = valueElement.Deserialize<TValue>(options);
				if (value is null) throw new JsonException($"The '{_valuePropertyName}' of a {nameof(Suspicious<TValue>)} can't be null");
				return Suspicious.Success(outcome, value);
			}

			return Suspicious.Success<TValue>(outcome);
		}
		catch (ArgumentException exception)
		{
			throw new JsonException($"The {nameof(Suspicious<TValue>)} payload is invalid for the outcome '{outcome}'", exception);
		}
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, Suspicious<TValue> value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WritePropertyName(_outcomePropertyName);
		JsonSerializer.Serialize(writer, value.Outcome, options);

		if (value.HasValue)
		{
			writer.WritePropertyName(_valuePropertyName);
			JsonSerializer.Serialize(writer, value.Value, options);
		}

		if (value.Error is not null)
		{
			writer.WritePropertyName(_errorPropertyName);
			JsonSerializer.Serialize(writer, value.Error, options);
		}

		writer.WriteEndObject();
	}
}