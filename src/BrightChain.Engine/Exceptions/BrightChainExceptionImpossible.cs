using System.Net;

namespace BrightChain.Engine.Exceptions;

/// <summary>
///     Base class for all BrightChain exceptions
/// </summary>
public class BrightChainExceptionImpossible : BrightChainException
{
    public BrightChainExceptionImpossible(string message) : base(message: message)
    {
    }

    public BrightChainExceptionImpossible(string message, HttpStatusCode statusCode, int subStatusCode, string activityId,
        double requestCharge) : base(message: message,
        statusCode: statusCode,
        subStatusCode: subStatusCode,
        activityId: activityId,
        requestCharge: requestCharge)
    {
    }
}
