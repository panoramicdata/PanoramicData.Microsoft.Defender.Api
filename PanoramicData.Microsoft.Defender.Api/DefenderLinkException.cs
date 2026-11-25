namespace PanoramicData.MicrosoftDefender.Api;

/// <summary>
/// Raised, before anything is sent, when a link taken from a response cannot be followed safely: a page's
/// <c>@odata.nextLink</c> that is not an <c>https</c> URL on the client's own host (so the bearer token is never sent
/// elsewhere), or an export file that is not an <c>https</c> URL. The message never contains the link's query string, which
/// can hold a signature.
/// </summary>
/// <param name="message">What was wrong with the link.</param>
public sealed class DefenderLinkException(string message) : InvalidOperationException(message);
