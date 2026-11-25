using Refit;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PanoramicData.MicrosoftDefender.Api.Serialization;

/// <summary>
/// Refit content serializer for Defender: request bodies are written as UTF-8 JSON with <see cref="DefenderJson.Options"/>
/// into a buffered <see cref="StringContent"/> (so the request can be replayed after a 401 or a retry), and responses are
/// read with the same options.
/// </summary>
internal sealed class DefenderContentSerializer : IHttpContentSerializer, IReflectionFieldNameProvider
{
	private readonly SystemTextJsonContentSerializer _json = new(DefenderJson.Options);

	/// <inheritdoc />
	public HttpContent ToHttpContent<T>(T item)
	{
		ArgumentNullException.ThrowIfNull(item);
		return new StringContent(
			JsonSerializer.Serialize(item, DefenderJson.Options),
			Encoding.UTF8,
			new MediaTypeHeaderValue("application/json"));
	}

	/// <inheritdoc />
	public Task<T?> FromHttpContentAsync<T>(HttpContent content, CancellationToken cancellationToken = default)
		=> _json.FromHttpContentAsync<T>(content, cancellationToken);

	/// <inheritdoc />
	public string? GetFieldNameForProperty(PropertyInfo propertyInfo)
		=> propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
}
