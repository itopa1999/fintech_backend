using Backend.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Extensions;

public static class ActionResultExtensions
{
    public static ActionResult<BaseResult> ToActionResult(this BaseResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        return new ObjectResult(result)
        {
            StatusCode = (int)result.StatusCode
        };
    }
}
