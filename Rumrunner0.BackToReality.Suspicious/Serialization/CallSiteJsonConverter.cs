namespace Rumrunner0.BackToReality.Suspicious.Serialization;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>JSON converter for <see cref="CallSite" />.</summary>
/// <remarks>Contract: <c>{"member":"CreateUser","filePath":"/src/UserService.cs","line":42}</c>.</remarks>
public sealed class CallSiteJsonConverter : JsonConverter<CallSite>
{
	/// <summary>JSON property name of the member.</summary>
	private const string _MEMBER_PROPERTY_NAME = "member";

	/// <summary>JSON property name of the file path.</summary>
	private const string _FILE_PATH_PROPERTY_NAME = "filePath";

	/// <summary>JSON property name of the line.</summary>
	private const string _LINE_PROPERTY_NAME = "line";

	/// <inheritdoc />
	public override CallSite Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using var document = JsonDocument.ParseValue(ref reader);
		var root = document.RootElement;

		if (root.ValueKind != JsonValueKind.Object) throw new JsonException($"A {nameof(CallSite)} must be a JSON object");

		var member = root.TryGetProperty(_MEMBER_PROPERTY_NAME, out var memberElement) && memberElement.ValueKind == JsonValueKind.String ? memberElement.GetString() : null;
		if (member is null) throw new JsonException($"A {nameof(CallSite)} requires a string '{_MEMBER_PROPERTY_NAME}'");

		var filePath = root.TryGetProperty(_FILE_PATH_PROPERTY_NAME, out var filePathElement) && filePathElement.ValueKind == JsonValueKind.String ? filePathElement.GetString() : null;
		if (filePath is null) throw new JsonException($"A {nameof(CallSite)} requires a string '{_FILE_PATH_PROPERTY_NAME}'");

		if (!root.TryGetProperty(_LINE_PROPERTY_NAME, out var lineElement) || !lineElement.TryGetInt32(out var line)) throw new JsonException($"A {nameof(CallSite)} requires an integer '{_LINE_PROPERTY_NAME}'");

		return CallSite.From(member, filePath, line);
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, CallSite value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WriteString(_MEMBER_PROPERTY_NAME, value.Member);
		writer.WriteString(_FILE_PATH_PROPERTY_NAME, value.FilePath);
		writer.WriteNumber(_LINE_PROPERTY_NAME, value.Line);
		writer.WriteEndObject();
	}
}