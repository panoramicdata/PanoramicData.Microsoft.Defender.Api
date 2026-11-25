using Microsoft.Extensions.Logging;

namespace PanoramicData.MicrosoftDefender.Api.Handlers;

/// <summary>Source-generated log messages. Only methods and paths are logged, never query strings, tokens or signatures.</summary>
internal static partial class Log
{
	[LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Defender {Method} {Path} (attempt {Attempt})")]
	public static partial void Sending(ILogger logger, HttpMethod method, string path, int attempt);

	[LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Defender returned {Status} for {Method} {Path}; retrying in {Delay}")]
	public static partial void Retrying(ILogger logger, int status, HttpMethod method, string path, TimeSpan delay);

	[LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Defender connection failed ({Error}) for {Method} {Path}; retrying in {Delay}")]
	public static partial void RetryingConnection(ILogger logger, HttpRequestError error, HttpMethod method, string path, TimeSpan delay);

	[LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Defender returned {Status} for {Method} {Path} with a Retry-After of {Delay}, which is longer than the configured maximum; not retrying")]
	public static partial void RetryAfterTooLong(ILogger logger, int status, HttpMethod method, string path, TimeSpan delay);
}
