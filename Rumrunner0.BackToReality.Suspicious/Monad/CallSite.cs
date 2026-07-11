using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Rumrunner0.BackToReality.SharedExtensions.Exceptions;
using Rumrunner0.BackToReality.Suspicious.Serialization;

namespace Rumrunner0.BackToReality.Suspicious.Monad;

/// <summary>Call site where an <see cref="Error" /> was created.</summary>
[JsonConverter(typeof(CallSiteJsonConverter))]
public sealed record class CallSite : IEquatable<CallSite>
{
	#region Instance State

	/// <summary>Member.</summary>
	private readonly string _member;

	/// <summary>File path.</summary>
	private readonly string _filePath;

	/// <summary>Line.</summary>
	private readonly int _line;

	/// <inheritdoc cref="CallSite" />
	private CallSite(string member, string filePath, int line)
	{
		ArgumentExceptionExtensions.ThrowIfNull(member);
		ArgumentExceptionExtensions.ThrowIfNull(filePath);

		this._member = member;
		this._filePath = filePath;
		this._line = line;
	}

	#endregion

	#region Common API

	/// <summary>Member.</summary>
	public string Member => this._member;

	/// <summary>File path.</summary>
	public string FilePath => this._filePath;

	/// <summary>File name.</summary>
	public string FileName => Path.GetFileName(this._filePath);

	/// <summary>Line.</summary>
	public int Line => this._line;

	#endregion

	#region Display

	/// <inheritdoc />
	public override string ToString() => $"at {this._member} in {this.FileName}, line {this._line}";

	#endregion

	#region Creation

	/// <summary>Captures the current call site.</summary>
	/// <param name="member">The caller member.</param>
	/// <param name="filePath">The caller file path.</param>
	/// <param name="line">The caller line.</param>
	/// <returns>A new <see cref="CallSite" />.</returns>
	public static CallSite Capture
	(
		[CallerMemberName] string member = "",
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int line = 0
	)
	{
		return new
		(
			member,
			filePath,
			line
		);
	}

	/// <summary>Creates a <see cref="CallSite" /> from already-captured caller details.</summary>
	/// <param name="member">The member.</param>
	/// <param name="filePath">The file path.</param>
	/// <param name="line">The line.</param>
	/// <returns>A new <see cref="CallSite" />.</returns>
	internal static CallSite From(string member, string filePath, int line)
	{
		return new
		(
			member,
			filePath,
			line
		);
	}

	#endregion
}