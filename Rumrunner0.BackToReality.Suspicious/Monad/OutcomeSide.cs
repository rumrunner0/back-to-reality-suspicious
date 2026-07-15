using System.Text.Json.Serialization;
using Ardalis.SmartEnum;
using Ardalis.SmartEnum.SystemTextJson;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Side of an <see cref="OutcomeKind" /> that declares on which rails it can be constructed.</summary>
/// <remarks>This is a construction-time guard only; the runtime truth of a result is the presence of an <see cref="Error" />.</remarks>
[JsonConverter(typeof(SmartEnumValueConverter<OutcomeSide, string>))]
public sealed class OutcomeSide : SmartEnum<OutcomeSide, string>
{
	/// <inheritdoc cref="OutcomeSide" />
	private OutcomeSide(string name, string value) : base(name, value) { }

	/// <summary>Flag that indicates whether this <see cref="OutcomeSide" /> allows the success rail.</summary>
	public bool AllowsSuccess => this == Success || this == Any;

	/// <summary>Flag that indicates whether this <see cref="OutcomeSide" /> allows the failure rail.</summary>
	public bool AllowsFailure => this == Failure || this == Any;

	/// <summary>Success side.</summary>
	public static readonly OutcomeSide Success = new (nameof(Success), "success");

	/// <summary>Failure side.</summary>
	public static readonly OutcomeSide Failure = new (nameof(Failure), "failure");

	/// <summary>Any side.</summary>
	public static readonly OutcomeSide Any = new (nameof(Any), "any");
}