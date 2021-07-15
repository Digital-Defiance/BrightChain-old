// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Globalization;
using System.Linq;
using System.Net;

namespace BrightChain.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class BrightChainExecutionStrategy : ExecutionStrategy
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BrightChainExecutionStrategy(
            DbContext context)
            : this(context, DefaultMaxRetryCount)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BrightChainExecutionStrategy(
            ExecutionStrategyDependencies dependencies)
            : this(dependencies, DefaultMaxRetryCount)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BrightChainExecutionStrategy(
            DbContext context,
            int maxRetryCount)
            : this(context, maxRetryCount, DefaultMaxDelay)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BrightChainExecutionStrategy(
            ExecutionStrategyDependencies dependencies,
            int maxRetryCount)
            : this(dependencies, maxRetryCount, DefaultMaxDelay)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BrightChainExecutionStrategy(DbContext context, int maxRetryCount, TimeSpan maxRetryDelay)
            : base(context, maxRetryCount, maxRetryDelay)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BrightChainExecutionStrategy(ExecutionStrategyDependencies dependencies, int maxRetryCount, TimeSpan maxRetryDelay)
            : base(dependencies, maxRetryCount, maxRetryDelay)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override bool ShouldRetryOn(Exception? exception)
        {
            return exception switch
            {
                BrightChainException brightChainException => IsTransient(brightChainException.StatusCode),
                HttpException httpException => IsTransient(httpException.Response.StatusCode),
                WebException webException => IsTransient(((HttpWebResponse)webException.Response!).StatusCode),
                _ => false
            };

            static bool IsTransient(HttpStatusCode statusCode)
                => statusCode == HttpStatusCode.ServiceUnavailable
                    || statusCode == HttpStatusCode.TooManyRequests;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override TimeSpan? GetNextDelay(Exception lastException)
        {
            var baseDelay = base.GetNextDelay(lastException);
            return baseDelay == null
                ? null
                : CallOnWrappedException(lastException, GetDelayFromException)
                    ?? baseDelay;
        }

        private static TimeSpan? GetDelayFromException(Exception? exception)
        {
            switch (exception)
            {
                case BrightChainException brightChainException:
                    return brightChainException.RetryAfter;

                case HttpException httpException:
                    {
                        if (httpException.Response.Headers.TryGetValues("x-ms-retry-after-ms", out var values)
                            && TryParseMsRetryAfter(values.FirstOrDefault(), out var delay))
                        {
                            return delay;
                        }

                        if (httpException.Response.Headers.TryGetValues("Retry-After", out values)
                            && TryParseRetryAfter(values.FirstOrDefault(), out delay))
                        {
                            return delay;
                        }

                        return null;
                    }

                case WebException webException:
                    {
                        var response = (HttpWebResponse)webException.Response!;

                        var delayString = response.Headers.GetValues("x-ms-retry-after-ms")?.FirstOrDefault();
                        if (TryParseMsRetryAfter(delayString, out var delay))
                        {
                            return delay;
                        }

                        delayString = response.Headers.GetValues("Retry-After")?.FirstOrDefault();
                        if (TryParseRetryAfter(delayString, out delay))
                        {
                            return delay;
                        }

                        return null;
                    }

                default:
                    return null;
            }

            static bool TryParseMsRetryAfter(string? delayString, out TimeSpan delay)
            {
                delay = default;
                if (delayString == null)
                {
                    return false;
                }

                if (int.TryParse(delayString, out var intDelay))
                {
                    delay = TimeSpan.FromMilliseconds(intDelay);
                    return true;
                }

                return false;
            }

            static bool TryParseRetryAfter(string? delayString, out TimeSpan delay)
            {
                delay = default;
                if (delayString == null)
                {
                    return false;
                }

                if (int.TryParse(delayString, out var intDelay))
                {
                    delay = TimeSpan.FromSeconds(intDelay);
                    return true;
                }

                if (DateTimeOffset.TryParse(delayString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var retryDate))
                {
                    delay = retryDate.Subtract(DateTimeOffset.Now);
                    delay = delay <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(1) : delay;
                    return true;
                }

                return false;
            }
        }
    }
}
