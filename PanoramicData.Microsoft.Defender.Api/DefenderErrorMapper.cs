using PanoramicData.MicrosoftDefender.Api.Handlers;
using System.Text.Json;

namespace PanoramicData.MicrosoftDefender.Api;

/// <summary>
/// Turns a non-success response into a <see cref="DefenderApiException"/>. Defender answers errors with
/// <c>{"error":{"code":"...","message":"...","target":"..."}}</c>, where the target is a correlation id. An empty, HTML or
/// otherwise unreadable body still yields an exception, with a message naming the status.
/// </summary>
internal static class DefenderErrorMapper
{
	public static async Task<Exception?> CreateAsync(HttpResponseMessage response, TimeProvider timeProvider)
	{
		if (response.IsSuccessStatusCode)
		{
			return null;
		}

		var body = await ReadBodyAsync(response).ConfigureAwait(false);
		var error = Parse(body);
		var message = error.Message is { Length: > 0 } text
			? text
			: $"HTTP {(int)response.StatusCode} ({response.ReasonPhrase ?? response.StatusCode.ToString()})";
		return new DefenderApiException(response.StatusCode, error.Code, message, error.Target, RetryAfterHeader.Read(response, timeProvider));
	}

	/// <summary>Reads the <c>error</c> object of an error body, tolerating a missing or malformed body.</summary>
	internal static ErrorBody Parse(string body)
	{
		try
		{
			using var document = JsonDocument.Parse(body);
			if (document.RootElement.ValueKind != JsonValueKind.Object
				|| !document.RootElement.TryGetProperty("error", out var error))
			{
				return default;
			}

			// Some services answer {"error":"text"} instead of the error object.
			return error.ValueKind == JsonValueKind.String
				? new ErrorBody(null, error.GetString(), null)
				: new ErrorBody(ReadString(error, "code"), ReadString(error, "message"), ReadString(error, "target"));
		}
		catch (JsonException)
		{
			return default;
		}
	}

	private static string? ReadString(JsonElement element, string name)
		=> element.ValueKind == JsonValueKind.Object
			&& element.TryGetProperty(name, out var property)
			&& property.ValueKind == JsonValueKind.String
			? property.GetString()
			: null;

	private static async Task<string> ReadBodyAsync(HttpResponseMessage response)
	{
		try
		{
			return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
		}
		catch (Exception exception) when (exception is InvalidOperationException or HttpRequestException or IOException)
		{
			// An unsupported charset in Content-Type, or a connection lost while reading: report the status, not the read.
			return string.Empty;
		}
	}

	/// <summary>The fields of Defender's error object.</summary>
	/// <param name="Code">The error code.</param>
	/// <param name="Message">The error message.</param>
	/// <param name="Target">The correlation id.</param>
	internal readonly record struct ErrorBody(string? Code, string? Message, string? Target);
}
