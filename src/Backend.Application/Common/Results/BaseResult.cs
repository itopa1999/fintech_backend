using System.Net;
using System.Text.Json.Serialization;

namespace Backend.Application.Common.Results;

public class BaseResult
{
    [JsonPropertyName("request_id")]
    [JsonPropertyOrder(1)]
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("message")]
    [JsonPropertyOrder(2)]
    public string Message { get; set; } =
        "An error occurred; please try again later";

    [JsonPropertyName("is_success")]
    [JsonPropertyOrder(3)]
    public bool IsSuccess =>
        ((int)StatusCode) >= 200 &&
        ((int)StatusCode) < 300;

    [JsonPropertyName("status_code")]
    [JsonPropertyOrder(5)]
    public HttpStatusCode StatusCode { get; set; } =
        HttpStatusCode.InternalServerError;

    public BaseResult()
    {
    }

    public BaseResult(
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError,
        string message = "An error occurred; please try again later",
        string? requestId = null)
    {
        StatusCode = statusCode;
        Message = message;
        RequestId = requestId ?? Guid.NewGuid().ToString();
    }

    public virtual Dictionary<string, object?> ToDictionary()
    {
        return new Dictionary<string, object?>
        {
            ["request_id"] = RequestId,
            ["message"] = Message,
            ["is_success"] = IsSuccess,
            ["status_code"] = (int)StatusCode
        };
    }
}

public class BaseResult<T> : BaseResult
{
    [JsonPropertyName("data")]
    [JsonPropertyOrder(4)]
    public T? Data { get; set; }

    public BaseResult()
    {
    }

    public BaseResult(
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError,
        string message = "An error occurred; please try again later",
        T? data = default,
        string? requestId = null)
        : base(statusCode, message, requestId)
    {
        Data = data;
    }

    public override Dictionary<string, object?> ToDictionary()
    {
        var result = base.ToDictionary();

        result["data"] = Data;

        return result;
    }
}