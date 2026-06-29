using System.Text.Json.Serialization;
using Ardalis.SmartEnum;
using Ardalis.SmartEnum.SystemTextJson;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary><see cref="Suspicious{TValue}" /> state.</summary>
[JsonConverter(typeof(SmartEnumValueConverter<SuspiciousState, string>))]
public sealed class SuspiciousState : SmartEnum<SuspiciousState, string>
{
	/// <inheritdoc cref="SuspiciousState" />
	private SuspiciousState(string name, string value) : base(name, value) { }

	/// <summary>Value.</summary>
	public new static readonly SuspiciousState Value = new (nameof(Value), "value");

	/// <summary>Error.</summary>
	public static readonly SuspiciousState Error = new (nameof(Error), "error");
}