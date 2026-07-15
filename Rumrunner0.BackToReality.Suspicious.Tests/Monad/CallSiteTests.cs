using Rumrunner0.BackToReality.Suspicious.Monad;
using Xunit;

namespace Rumrunner0.BackToReality.Suspicious.Tests.Monad;

/// <summary>Tests for <see cref="CallSite" />.</summary>
public sealed class CallSiteTests
{
	#region Creation

	/// <summary>Ensures that <see cref="CallSite.Capture" /> captures the caller details.</summary>
	[Fact]
	public void Capture_CapturesCallerDetails()
	{
		var site = CallSite.Capture();

		Assert.Equal(nameof(this.Capture_CapturesCallerDetails), site.Member);
		Assert.Equal($"{nameof(CallSiteTests)}.cs", site.FileName);
		Assert.Contains(site.FileName, site.FilePath);
		Assert.True(site.Line > 0);
	}

	#endregion

	#region Equality

	/// <summary>Ensures that equality is structural.</summary>
	[Fact]
	public void Equality_IsStructural()
	{
		var site = CallSite.Capture();

		Assert.Equal(site, site with { });
		Assert.NotEqual(site, CallSite.Capture());
	}

	#endregion

	#region Display

	/// <summary>Ensures that <see cref="CallSite.ToString" /> contains the member, the file name and the line.</summary>
	[Fact]
	public void ToString_ContainsMemberFileNameAndLine()
	{
		var site = CallSite.Capture();
		var text = site.ToString();

		Assert.StartsWith($"at {site.Member} in {site.FileName}, line ", text);
		Assert.EndsWith($"{site.Line}", text);
	}

	#endregion
}