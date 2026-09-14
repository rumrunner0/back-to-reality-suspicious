namespace Rumrunner0.BackToReality.Suspicious.Serialization;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>JSON converter for <see cref="OutcomeKind" />.</summary>
/// <remarks>Contract: <c>{"name":"invalid","code":1000,"side":"failure"}</c>. Presets deserialize to their singleton instances; other kinds go through <see cref="OutcomeKind.Custom" /> with strict range validation.</remarks>
public sealed class OutcomeKindJsonConverter : JsonConverter<OutcomeKind>
{
	/// <summary>JSON property name of the name.</summary>
	private const string _NAME_PROPERTY_NAME = "name";

	/// <summary>JSON property name of the code.</summary>
	private const string _CODE_PROPERTY_NAME = "code";

	/// <summary>JSON property name of the side.</summary>
	private const string _SIDE_PROPERTY_NAME = "side";

	/// <inheritdoc />
	public override OutcomeKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using var document = JsonDocument.ParseValue(ref reader);
		var root = document.RootElement;

		if (root.ValueKind != JsonValueKind.Object) throw new JsonException($"An {nameof(OutcomeKind)} must be a JSON object");

		var name = root.TryGetProperty(_NAME_PROPERTY_NAME, out var nameElement) && nameElement.ValueKind == JsonValueKind.String ? nameElement.GetString() : null;
		if (name is null) throw new JsonException($"An {nameof(OutcomeKind)} requires a string '{_NAME_PROPERTY_NAME}'");

		if (!root.TryGetProperty(_CODE_PROPERTY_NAME, out var codeElement) || !codeElement.TryGetInt32(out var code)) throw new JsonException($"An {nameof(OutcomeKind)} requires an integer '{_CODE_PROPERTY_NAME}'");

		var sideValue = root.TryGetProperty(_SIDE_PROPERTY_NAME, out var sideElement) && sideElement.ValueKind == JsonValueKind.String ? sideElement.GetString() : null;
		if (sideValue is null || !OutcomeSide.TryFromValue(sideValue, out var side)) throw new JsonException($"An {nameof(OutcomeKind)} requires a valid '{_SIDE_PROPERTY_NAME}'");

		foreach (var preset in OutcomeKind.Presets)
		{
			if (preset.Name == name && preset.Code == code && preset.Side == side) return preset;
		}

		try
		{
			return OutcomeKind.Custom(name, code, side);
		}
		catch (ArgumentException exception)
		{
			throw new JsonException($"The outcome kind '{name} ({code})' is neither a preset nor a valid custom kind", exception);
		}
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, OutcomeKind value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WriteString(_NAME_PROPERTY_NAME, value.Name);
		writer.WriteNumber(_CODE_PROPERTY_NAME, value.Code);
		writer.WriteString(_SIDE_PROPERTY_NAME, value.Side.Value);
		writer.WriteEndObject();
	}
}