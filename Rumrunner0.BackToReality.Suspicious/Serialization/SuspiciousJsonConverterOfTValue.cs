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
	private const string _OUTCOME_PROPERTY_NAME = "outcome";

	/// <summary>JSON property name of the value.</summary>
	private const string _VALUE_PROPERTY_NAME = "value";

	/// <summary>JSON property name of the error.</summary>
	private const string _ERROR_PROPERTY_NAME = "error";

	/// <inheritdoc />
	public override Suspicious<TValue> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using var document = JsonDocument.ParseValue(ref reader);
		var root = document.RootElement;

		if (root.ValueKind != JsonValueKind.Object) throw new JsonException($"A {nameof(Suspicious<TValue>)} must be a JSON object");

		if (!root.TryGetProperty(_OUTCOME_PROPERTY_NAME, out var outcomeElement)) throw new JsonException($"A {nameof(Suspicious<TValue>)} requires an '{_OUTCOME_PROPERTY_NAME}'");
		var outcome = outcomeElement.Deserialize<OutcomeKind>(options);
		if (outcome is null) throw new JsonException($"A {nameof(Suspicious<TValue>)} requires a non-null '{_OUTCOME_PROPERTY_NAME}'");

		var hasValue = root.TryGetProperty(_VALUE_PROPERTY_NAME, out var valueElement);
		var error = root.TryGetProperty(_ERROR_PROPERTY_NAME, out var errorElement) && errorElement.ValueKind == JsonValueKind.Object ? errorElement.Deserialize<Error>(options) : null;

		if (hasValue && error is not null) throw new JsonException($"A {nameof(Suspicious<TValue>)} can't have both a '{_VALUE_PROPERTY_NAME}' and an '{_ERROR_PROPERTY_NAME}'");

		if (error is not null)
		{
			if (error.Kind != outcome) throw new JsonException($"The '{_OUTCOME_PROPERTY_NAME}' ({outcome}) doesn't match the '{_ERROR_PROPERTY_NAME}' kind ({error.Kind})");
			return Suspicious.Fail<TValue>(error);
		}

		try
		{
			if (hasValue)
			{
				var value = valueElement.Deserialize<TValue>(options);
				if (value is null) throw new JsonException($"The '{_VALUE_PROPERTY_NAME}' of a {nameof(Suspicious<TValue>)} can't be null");
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

		writer.WritePropertyName(_OUTCOME_PROPERTY_NAME);
		JsonSerializer.Serialize(writer, value.Outcome, options);

		if (value.HasValue)
		{
			writer.WritePropertyName(_VALUE_PROPERTY_NAME);
			JsonSerializer.Serialize(writer, value.Value, options);
		}

		if (value.Error is not null)
		{
			writer.WritePropertyName(_ERROR_PROPERTY_NAME);
			JsonSerializer.Serialize(writer, value.Error, options);
		}

		writer.WriteEndObject();
	}
}