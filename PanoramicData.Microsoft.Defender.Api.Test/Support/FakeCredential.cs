using Azure.Core;

namespace PanoramicData.MicrosoftDefender.Api.Test.Support;

/// <summary>
/// A <see cref="TokenCredential"/> that hands out <c>token-1</c>, <c>token-2</c>... (each valid for <see cref="Lifetime"/>
/// from <see cref="Clock"/>'s time) and records the scopes it was asked for. It never calls Azure.
/// </summary>
internal sealed class FakeCredential(TimeProvider clock) : TokenCredential
{
	private int _issued;

	/// <summary>How long each issued token is valid for.</summary>
	public TimeSpan Lifetime { get; set; } = TimeSpan.FromHours(1);

	/// <summary>When set, every request returns this token instead of a numbered one (a credential that caches).</summary>
	public string? FixedToken { get; set; }

	/// <summary>An optional gate a test can hold to keep a token request in flight.</summary>
	public Task? Gate { get; set; }

	/// <summary>Completed when the first token request arrives.</summary>
	public TaskCompletionSource FirstRequested { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

	public List<string[]> RequestedScopes { get; } = [];

	public int Issued => Volatile.Read(ref _issued);

	public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
		=> GetTokenAsync(requestContext, cancellationToken).AsTask().GetAwaiter().GetResult();

	public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
	{
		lock (RequestedScopes)
		{
			RequestedScopes.Add(requestContext.Scopes);
		}

		FirstRequested.TrySetResult();
		if (Gate is { } gate)
		{
			await gate.WaitAsync(cancellationToken);
		}

		var number = Interlocked.Increment(ref _issued);
		return new AccessToken(FixedToken ?? $"token-{number}", clock.GetUtcNow() + Lifetime);
	}
}
