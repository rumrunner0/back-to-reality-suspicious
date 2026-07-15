namespace Rumrunner0.BackToReality.Suspicious.Serialization;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>JSON converter for <see cref="Error" />.</summary>
/// <remarks>The <c>exception</c> member is written as <c>{"type":…,"message":…}</c> for observability and deserializes to <c>null</c> — a documented lossy round-trip, since a real <see cref="Exception" /> can't be faithfully rebuilt.</remarks>
public sealed class ErrorJsonConverter : JsonConverter<Error>
{
	/// <summary>JSON property name of the kind.</summary>
	private const string _kindPropertyName = "kind";

	/// <summary>JSON property name of the description.</summary>
	private const string _descriptionPropertyName = "description";

	/// <summary>JSON property name of the exception.</summary>
	private const string _exceptionPropertyName = "exception";

	/// <summary>JSON property name of the exception type.</summary>
	private const string _exceptionTypePropertyName = "type";

	/// <summary>JSON property name of the exception message.</summary>
	private const string _exceptionMessagePropertyName = "message";

	/// <summary>JSON property name of the site.</summary>
	private const string _sitePropertyName = "site";

	/// <summary>JSON property name of the cause.</summary>
	private const string _causePropertyName = "cause";

	/// <summary>JSON property name of the details.</summary>
	private const string _detailsPropertyName = "details";

	/// <inheritdoc />
	public override Error Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using var document = JsonDocument.ParseValue(ref reader);
		var root = document.RootElement;

		if (root.ValueKind != JsonValueKind.Object) throw new JsonException($"An {nameof(Error)} must be a JSON object");

		if (!root.TryGetProperty(_kindPropertyName, out var kindElement)) throw new JsonException($"An {nameof(Error)} requires a '{_kindPropertyName}'");
		var kind = kindElement.Deserialize<OutcomeKind>(options);
		if (kind is null) throw new JsonException($"An {nameof(Error)} requires a non-null '{_kindPropertyName}'");

		var description = root.TryGetProperty(_descriptionPropertyName, out var descriptionElement) && descriptionElement.ValueKind == JsonValueKind.String ? descriptionElement.GetString() : null;
		var site = root.TryGetProperty(_sitePropertyName, out var siteElement) && siteElement.ValueKind == JsonValueKind.Object ? siteElement.Deserialize<CallSite>(options) : null;
		var cause = root.TryGetProperty(_causePropertyName, out var causeElement) && causeElement.ValueKind == JsonValueKind.Object ? causeElement.Deserialize<Error>(options) : null;

		var details = default(List<Error>);
		if (root.TryGetProperty(_detailsPropertyName, out var detailsElement) && detailsElement.ValueKind == JsonValueKind.Array)
		{
			details = [];

			foreach (var detailElement in detailsElement.EnumerateArray())
			{
				if (detailElement.Deserialize<Error>(options) is { } detail) details.Add(detail);
			}
		}

		try
		{
			return Error.From
			(
				kind,
				description,
				site,
				cause,
				details
			);
		}
		catch (ArgumentException exception)
		{
			throw new JsonException($"The {nameof(Error)} payload is invalid for the kind '{kind}'", exception);
		}
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, Error value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();

		writer.WritePropertyName(_kindPropertyName);
		JsonSerializer.Serialize(writer, value.Kind, options);

		if (value.Description is not null) writer.WriteString(_descriptionPropertyName, value.Description);

		if (value.Exception is not null)
		{
			writer.WriteStartObject(_exceptionPropertyName);
			writer.WriteString(_exceptionTypePropertyName, value.Exception.GetType().FullName);
			writer.WriteString(_exceptionMessagePropertyName, value.Exception.Message);
			writer.WriteEndObject();
		}

		if (value.Site is not null)
		{
			writer.WritePropertyName(_sitePropertyName);
			JsonSerializer.Serialize(writer, value.Site, options);
		}

		if (value.Cause is not null)
		{
			writer.WritePropertyName(_causePropertyName);
			JsonSerializer.Serialize(writer, value.Cause, options);
		}

		if (value.Details.Count > 0)
		{
			writer.WriteStartArray(_detailsPropertyName);
			foreach (var detail in value.Details) JsonSerializer.Serialize(writer, detail, options);
			writer.WriteEndArray();
		}

		writer.WriteEndObject();
	}
}