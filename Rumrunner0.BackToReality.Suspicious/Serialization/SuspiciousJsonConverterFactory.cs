namespace Rumrunner0.BackToReality.Suspicious.Serialization;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>JSON converter factory for <see cref="Suspicious{TValue}" /> — creates a <see cref="SuspiciousJsonConverterOfTValue{TValue}" /> per closed generic.</summary>
public sealed class SuspiciousJsonConverterFactory : JsonConverterFactory
{
	/// <inheritdoc />
	public override bool CanConvert(Type typeToConvert)
	{
		return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Suspicious<>);
	}

	/// <inheritdoc />
	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
	{
		var valueType = typeToConvert.GetGenericArguments()[0];
		return (JsonConverter)Activator.CreateInstance(typeof(SuspiciousJsonConverterOfTValue<>).MakeGenericType(valueType))!;
	}
}