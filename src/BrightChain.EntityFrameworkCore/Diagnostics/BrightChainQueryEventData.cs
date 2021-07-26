// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BrightChain.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for BrightChain query events.
    /// </summary>
    public class BrightChainQueryEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="containerId"> The ID of the BrightChain container being queried. </param>
        /// <param name="partitionKey"> The key of the BrightChain partition that the query is using. </param>
        /// <param name="parameters"> Name/values for each parameter in the BrightChain Query. </param>
        /// <param name="querySql"> The SQL representing the query. </param>
        /// <param name="logSensitiveData"> Indicates whether or not the application allows logging of sensitive data. </param>
        public BrightChainQueryEventData(
            EventDefinitionBase eventDefinition,
            Func<EventDefinitionBase, EventData, string> messageGenerator,
            string containerId,
            string? partitionKey,
            IReadOnlyList<(string Name, object? Value)> parameters,
            string querySql,
            bool logSensitiveData)
            : base(eventDefinition, messageGenerator)
        {
            this.ContainerId = containerId;
            this.PartitionKey = partitionKey;
            this.Parameters = parameters;
            this.QuerySql = querySql;
            this.LogSensitiveData = logSensitiveData;
        }

        /// <summary>
        ///     The ID of the BrightChain container being queried.
        /// </summary>
        public virtual string ContainerId { get; }

        /// <summary>
        ///     The key of the BrightChain partition that the query is using.
        /// </summary>
        public virtual string? PartitionKey { get; }

        /// <summary>
        ///     Name/values for each parameter in the BrightChain Query.
        /// </summary>
        public virtual IReadOnlyList<(string Name, object? Value)> Parameters { get; }

        /// <summary>
        ///     The SQL representing the query.
        /// </summary>
        public virtual string QuerySql { get; }

        /// <summary>
        ///     Indicates whether or not the application allows logging of sensitive data.
        /// </summary>
        public virtual bool LogSensitiveData { get; }
    }
}
