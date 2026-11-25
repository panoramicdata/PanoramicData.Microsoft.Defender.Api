namespace PanoramicData.MicrosoftDefender.Api;

/// <summary>
/// Raised, before anything is sent, when a client created with <see cref="DefenderClientOptions.IsReadOnly"/> is asked to
/// make a request that could change Defender.
/// </summary>
/// <param name="method">The refused HTTP method.</param>
/// <param name="path">The refused request path (without the query string).</param>
public sealed class DefenderReadOnlyException(string method, string path)
	: InvalidOperationException($"The Defender client is read-only and refused {method} {path}.")
{
	/// <summary>The refused HTTP method.</summary>
	public string Method { get; } = method;

	/// <summary>The refused request path, without the query string.</summary>
	public string Path { get; } = path;
}
