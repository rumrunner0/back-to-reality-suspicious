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
	private const string _KIND_PROPERTY_NAME = "kind";

	/// <summary>JSON property name of the description.</summary>
	private const string _DESCRIPTION_PROPERTY_NAME = "description";

	/// <summary>JSON property name of the exception.</summary>
	private const string _EXCEPTION_PROPERTY_NAME = "exception";

	/// <summary>JSON property name of the exception type.</summary>
	private const string _EXCEPTION_TYPE_PROPERTY_NAME = "type";

	/// <summary>JSON property name of the exception message.</summary>
	private const string _EXCEPTION_MESSAGE_PROPERTY_NAME = "message";

	/// <summary>JSON property name of the site.</summary>
	private const string _SITE_PROPERTY_NAME = "site";

	/// <summary>JSON property name of the cause.</summary>
	private const string _CAUSE_PROPERTY_NAME = "cause";

	/// <summary>JSON property name of the details.</summary>
	private const string _DETAILS_PROPERTY_NAME = "details";

	/// <inheritdoc />
	public override Error Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using var document = JsonDocument.ParseValue(ref reader);
		var root = document.RootElement;

		if (root.ValueKind != JsonValueKind.Object) throw new JsonException($"An {nameof(Error)} must be a JSON object");

		if (!root.TryGetProperty(_KIND_PROPERTY_NAME, out var kindElement)) throw new JsonException($"An {nameof(Error)} requires a '{_KIND_PROPERTY_NAME}'");
		var kind = kindElement.Deserialize<OutcomeKind>(options);
		if (kind is null) throw new JsonException($"An {nameof(Error)} requires a non-null '{_KIND_PROPERTY_NAME}'");

		var description = root.TryGetProperty(_DESCRIPTION_PROPERTY_NAME, out var descriptionElement) && descriptionElement.ValueKind == JsonValueKind.String ? descriptionElement.GetString() : null;
		var site = root.TryGetProperty(_SITE_PROPERTY_NAME, out var siteElement) && siteElement.ValueKind == JsonValueKind.Object ? siteElement.Deserialize<CallSite>(options) : null;
		var cause = root.TryGetProperty(_CAUSE_PROPERTY_NAME, out var causeElement) && causeElement.ValueKind == JsonValueKind.Object ? causeElement.Deserialize<Error>(options) : null;

		var details = default(List<Error>);
		if (root.TryGetProperty(_DETAILS_PROPERTY_NAME, out var detailsElement) && detailsElement.ValueKind == JsonValueKind.Array)
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

		writer.WritePropertyName(_KIND_PROPERTY_NAME);
		JsonSerializer.Serialize(writer, value.Kind, options);

		if (value.Description is not null) writer.WriteString(_DESCRIPTION_PROPERTY_NAME, value.Description);

		if (value.Exception is not null)
		{
			writer.WriteStartObject(_EXCEPTION_PROPERTY_NAME);
			writer.WriteString(_EXCEPTION_TYPE_PROPERTY_NAME, value.Exception.GetType().FullName);
			writer.WriteString(_EXCEPTION_MESSAGE_PROPERTY_NAME, value.Exception.Message);
			writer.WriteEndObject();
		}

		if (value.Site is not null)
		{
			writer.WritePropertyName(_SITE_PROPERTY_NAME);
			JsonSerializer.Serialize(writer, value.Site, options);
		}

		if (value.Cause is not null)
		{
			writer.WritePropertyName(_CAUSE_PROPERTY_NAME);
			JsonSerializer.Serialize(writer, value.Cause, options);
		}

		if (value.Details.Count > 0)
		{
			writer.WriteStartArray(_DETAILS_PROPERTY_NAME);
			foreach (var detail in value.Details) JsonSerializer.Serialize(writer, detail, options);
			writer.WriteEndArray();
		}

		writer.WriteEndObject();
	}
}