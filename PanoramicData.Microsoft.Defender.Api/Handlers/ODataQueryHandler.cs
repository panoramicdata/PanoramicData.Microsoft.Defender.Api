namespace PanoramicData.MicrosoftDefender.Api.Handlers;

/// <summary>
/// Writes the <c>$</c> of OData system query options (<c>$filter</c>, <c>$top</c>, <c>$skip</c>, <c>$expand</c>) literally in
/// the query string. Refit escapes query names, so it sends <c>%24filter</c>, which OData services are not required to
/// understand. Only the first character of a name is touched, and only when it is an escaped <c>$</c>; values are left as
/// Refit escaped them. The URL is rebuilt by concatenating strings, never with <see cref="UriBuilder"/> setters, which would
/// escape the existing <c>%</c> signs a second time.
/// </summary>
internal sealed class ODataQueryHandler : DelegatingHandler
{
	private const string EscapedDollar = "%24";

	/// <inheritdoc />
	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var uri = request.RequestUri!;
		if (uri.Query.Contains(EscapedDollar, StringComparison.OrdinalIgnoreCase))
		{
			var query = string.Join('&', uri.Query[1..].Split('&').Select(UnescapeLeadingDollar));
			request.RequestUri = new Uri(uri.GetLeftPart(UriPartial.Path) + "?" + query + uri.Fragment);
		}

		return base.SendAsync(request, cancellationToken);
	}

	private static string UnescapeLeadingDollar(string pair)
		=> pair.StartsWith(EscapedDollar, StringComparison.OrdinalIgnoreCase) ? "$" + pair[EscapedDollar.Length..] : pair;
}
