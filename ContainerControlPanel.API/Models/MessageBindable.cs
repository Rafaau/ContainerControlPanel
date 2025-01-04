using Google.Protobuf;
using Microsoft.Extensions.Primitives;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Http.Headers;

namespace ContainerControlPanel.API.Models;

/// <summary>
/// Represents a message bindable from HTTP context.
/// </summary>
/// <typeparam name="TMessage">Generic message type</typeparam>
public sealed class MessageBindable<TMessage> : IBindableFromHttpContext<MessageBindable<TMessage>> where TMessage : IMessage<TMessage>, new()
{
    /// <summary>
    /// Empty message bindable
    /// </summary>
    public static readonly MessageBindable<TMessage> Empty = new MessageBindable<TMessage>();

    /// <summary>
    /// Gets the message
    /// </summary>
    public TMessage? Message { get; private set; }

    #region Constants

    /// <summary>
    /// Protobuf content type
    /// </summary>
    public const string ProtobufContentType = "application/x-protobuf";

    /// <summary>
    /// JSON content type
    /// </summary>
    public const string JsonContentType = "application/json";

    /// <summary>
    /// CORS policy name
    /// </summary>
    public const string CorsPolicyName = "OtlpHttpCors";

    #endregion

    /// <summary>
    /// Binds a message from the HTTP context.
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <param name="parameter">Parameter info</param>
    /// <returns>Returns a message bindable</returns>
    public static async ValueTask<MessageBindable<TMessage>?> BindAsync(HttpContext context, System.Reflection.ParameterInfo parameter)
    {
        switch (GetKnownContentType(context.Request.ContentType, out var charSet))
        {
            case KnownContentType.Protobuf:
                try
                {
                    var message = await ReadOtlpData(context, data =>
                    {
                        var message = new TMessage();
                        message.MergeFrom(data);
                        return message;
                    }).ConfigureAwait(false);

                    return new MessageBindable<TMessage> { Message = message };
                }
                catch (BadHttpRequestException ex)
                {
                    context.Response.StatusCode = ex.StatusCode;
                    return Empty;
                }
            case KnownContentType.Json:
            default:
                context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                return Empty;
        }
    }

    /// <summary>
    /// Gets the known content type.
    /// </summary>
    /// <param name="contentType">Content type</param>
    /// <param name="charSet">String segment for character set</param>
    /// <returns>Returns the known content type</returns>
    private static KnownContentType GetKnownContentType(string? contentType, out StringSegment charSet)
    {
        if (contentType != null && MediaTypeHeaderValue.TryParse(contentType, out var mt))
        {
            if (string.Equals(mt.MediaType, JsonContentType, StringComparison.OrdinalIgnoreCase))
            {
                charSet = mt.CharSet;
                return KnownContentType.Json;
            }

            if (string.Equals(mt.MediaType, ProtobufContentType, StringComparison.OrdinalIgnoreCase))
            {
                charSet = mt.CharSet;
                return KnownContentType.Protobuf;
            }
        }

        charSet = default;
        return KnownContentType.None;
    }

    /// <summary>
    /// Reads OTLP data.
    /// </summary>
    /// <typeparam name="T">Generic type</typeparam>
    /// <param name="httpContext">HTTP context</param>
    /// <param name="exporter">Exporter function</param>
    /// <returns>Returns the OTLP data</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled</exception>
    /// <exception cref="BadHttpRequestException">Thrown when the request is bad</exception>
    private static async Task<T> ReadOtlpData<T>(HttpContext httpContext, Func<ReadOnlySequence<byte>, T> exporter)
    {
        const int MaxRequestSize = 1024 * 1024 * 4; // 4 MB. Matches default gRPC request limit.

        ReadResult result = default;
        try
        {
            do
            {
                result = await httpContext.Request.BodyReader.ReadAsync().ConfigureAwait(false);

                if (result.IsCanceled)
                {
                    throw new OperationCanceledException("Read call was canceled.");
                }

                if (result.Buffer.Length > MaxRequestSize)
                {
                    // Too big!
                    throw new BadHttpRequestException(
                        $"The request body was larger than the max allowed of {MaxRequestSize} bytes.",
                        StatusCodes.Status400BadRequest);
                }

                if (result.IsCompleted)
                {
                    break;
                }
                else
                {
                    httpContext.Request.BodyReader.AdvanceTo(result.Buffer.Start, result.Buffer.End);
                }
            } while (true);

            return exporter(result.Buffer);
        }
        finally
        {
            if (!result.Equals(default(ReadResult)))
            {
                httpContext.Request.BodyReader.AdvanceTo(result.Buffer.End);
            }
        }
    }

    /// <summary>
    /// Known content type.
    /// </summary>
    private enum KnownContentType
    {
        None,
        Protobuf,
        Json
    }
}
