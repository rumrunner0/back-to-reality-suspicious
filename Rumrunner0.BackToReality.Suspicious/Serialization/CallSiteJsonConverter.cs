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
	private const string _memberPropertyName = "member";

	/// <summary>JSON property name of the file path.</summary>
	private const string _filePathPropertyName = "filePath";

	/// <summary>JSON property name of the line.</summary>
	private const string _linePropertyName = "line";

	/// <inheritdoc />
	public override CallSite Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using var document = JsonDocument.ParseValue(ref reader);
		var root = document.RootElement;

		if (root.ValueKind != JsonValueKind.Object) throw new JsonException($"A {nameof(CallSite)} must be a JSON object");

		var member = root.TryGetProperty(_memberPropertyName, out var memberElement) && memberElement.ValueKind == JsonValueKind.String ? memberElement.GetString() : null;
		if (member is null) throw new JsonException($"A {nameof(CallSite)} requires a string '{_memberPropertyName}'");

		var filePath = root.TryGetProperty(_filePathPropertyName, out var filePathElement) && filePathElement.ValueKind == JsonValueKind.String ? filePathElement.GetString() : null;
		if (filePath is null) throw new JsonException($"A {nameof(CallSite)} requires a string '{_filePathPropertyName}'");

		if (!root.TryGetProperty(_linePropertyName, out var lineElement) || !lineElement.TryGetInt32(out var line)) throw new JsonException($"A {nameof(CallSite)} requires an integer '{_linePropertyName}'");

		return CallSite.From(member, filePath, line);
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, CallSite value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WriteString(_memberPropertyName, value.Member);
		writer.WriteString(_filePathPropertyName, value.FilePath);
		writer.WriteNumber(_linePropertyName, value.Line);
		writer.WriteEndObject();
	}
}