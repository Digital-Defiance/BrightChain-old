using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;

namespace BrightChain.Engine.Exceptions
{
    /// <summary>
    /// Base class for all BrightChain exceptions
    /// </summary>
    public class BrightChainException : Exception
    {
        public BrightChainException(string message) : base(message)
        {
            this.StackTrace = new StackTrace().ToString();
        }

        //
        // Summary:
        //     Create a BrightChainException
        //
        // Parameters:
        //   message:
        //     The message associated with the exception.
        //
        //   statusCode:
        //     The System.Net.HttpStatusCode associated with the exception.
        //
        //   subStatusCode:
        //     A sub status code associated with the exception.
        //
        //   activityId:
        //     An ActivityId associated with the operation that generated the exception.
        //
        //   requestCharge:
        //     A request charge associated with the operation that generated the exception.
        public BrightChainException(string message, HttpStatusCode statusCode, int subStatusCode, string activityId, double requestCharge)
        {

        }

        //
        // Summary:
        //     The body of the bright chain response message as a string
        public virtual string ResponseBody { get; }
        //
        // Summary:
        //     Gets the request completion status code from the BrightChain service.
        //
        // Value:
        //     The request completion status code
        public virtual HttpStatusCode StatusCode { get; }
        //
        // Summary:
        //     Gets the request completion sub status code from the BrightChain service.
        //
        // Value:
        //     The request completion status code
        public virtual int SubStatusCode { get; }
        //
        // Summary:
        //     Gets the request charge for this request from the BrightChain service.
        //
        // Value:
        //     The request charge measured in request units.
        public virtual double RequestCharge { get; }
        //
        // Summary:
        //     Gets the activity ID for the request from the BrightChain service.
        //
        // Value:
        //     The activity ID for the request.
        public virtual string ActivityId { get; }
        //
        // Summary:
        //     Gets the retry after time. This tells how long a request should wait before doing
        //     a retry.
        public virtual TimeSpan? RetryAfter { get; }

        //
        // Summary:
        //     Gets the response headers
        public virtual HttpHeaders Headers { get; }
        public override string StackTrace { get; }

        //
        // Summary:
        //     Create a custom string with all the relevant exception information
        //
        // Returns:
        //     A string representation of the exception.
        public override string ToString()
        {
            throw new NotImplementedException();
        }

        //
        // Summary:
        //     Try to get a header from the brightchain response message
        //
        // Parameters:
        //   headerName:
        //
        //   value:
        //
        // Returns:
        //     A value indicating if the header was read.
        public virtual bool TryGetHeader(string headerName, out string value)
        {
            throw new NotImplementedException();
        }
    }
}


