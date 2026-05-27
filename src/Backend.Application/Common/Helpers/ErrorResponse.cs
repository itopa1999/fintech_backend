using Newtonsoft.Json;
using System.Text.Json.Serialization;
using static Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary;

namespace GateForce.Shared.Infrastructure.Helpers;

public class ErrorResponse
{
    [JsonProperty("error_description")]
    [JsonPropertyName("error_description")]
    public string ErrorDescription { get; set; } = "An error  occured, try again later.";

    public static ErrorResponse GetModelStateErrors(ValueEnumerable errors)
    {
        var message = string.Join(" | ", errors
                           .SelectMany(v => v.Errors)
                           .Select(e => e.ErrorMessage));
        return new ErrorResponse { ErrorDescription = !string.IsNullOrEmpty(message) ? message : "Fill required values" };
    }
}

public class ErrorResponse<TBody> : ErrorResponse
{
    [JsonProperty("error_body")]
    [JsonPropertyName("error_body")]
    public TBody? ErrorBody { get; set; }

    public static new ErrorResponse<TBody> GetModelStateErrors(ValueEnumerable errors)
    {
        var message = string.Join(" | ", errors
                           .SelectMany(v => v.Errors)
                           .Select(e => e.ErrorMessage));
        return new ErrorResponse<TBody> { ErrorDescription = !string.IsNullOrEmpty(message) ? message : "Fill required values" };
    }
}

#region User-Defined Exceptions
[Serializable]
public class CustomException : Exception
{
    public CustomException(string message) : base(message)
    {
    }

    public CustomException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public CustomException()
    {
    }
}
#endregion
