// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Internal;
using BrightChain.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;

// ReSharper disable once CheckNamespace
namespace BrightChain.EntityFrameworkCore.Diagnostics.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class BrightChainLoggerExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ExecutingSqlQuery(
            this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            string containerId,
            string? partitionKey,
            BrightChainSqlQuery brightChainSqlQuery)
        {
            var definition = BrightChainResources.LogExecutingSqlQuery(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                var logSensitiveData = diagnostics.ShouldLogSensitiveData();

                definition.Log(
                    diagnostics,
                    containerId,
                    logSensitiveData ? partitionKey : "?",
                    FormatParameters(brightChainSqlQuery.Parameters, logSensitiveData && brightChainSqlQuery.Parameters.Count > 0),
                    Environment.NewLine,
                    brightChainSqlQuery.Query);
            }

            if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = new BrightChainQueryEventData(
                    definition,
                    ExecutingSqlQuery,
                    containerId,
                    partitionKey,
                    brightChainSqlQuery.Parameters.Select(p => (p.Name, p.Value)).ToList(),
                    brightChainSqlQuery.Query,
                    diagnostics.ShouldLogSensitiveData());

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }

        private static string ExecutingSqlQuery(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string?, string, string, string>)definition;
            var p = (BrightChainQueryEventData)payload;
            return d.GenerateMessage(
                p.ContainerId,
                p.LogSensitiveData ? p.PartitionKey : "?",
                FormatParameters(p.Parameters, p.LogSensitiveData && p.Parameters.Count > 0),
                Environment.NewLine,
                p.QuerySql);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static void ExecutingReadItem(
            this IDiagnosticsLogger<DbLoggerCategory.Database.Command> diagnostics,
            string containerId,
            string? partitionKey,
            string resourceId)
        {
            var definition = BrightChainResources.LogExecutingReadItem(diagnostics);

            if (diagnostics.ShouldLog(definition))
            {
                var logSensitiveData = diagnostics.ShouldLogSensitiveData();
                definition.Log(diagnostics, logSensitiveData ? resourceId : "?", containerId, logSensitiveData ? partitionKey : "?");
            }

            if (diagnostics.NeedsEventData(definition, out var diagnosticSourceEnabled, out var simpleLogEnabled))
            {
                var eventData = new BrightChainReadItemEventData(
                    definition,
                    ExecutingReadItem,
                    resourceId,
                    containerId,
                    partitionKey,
                    diagnostics.ShouldLogSensitiveData());

                diagnostics.DispatchEventData(definition, eventData, diagnosticSourceEnabled, simpleLogEnabled);
            }
        }

        private static string ExecutingReadItem(EventDefinitionBase definition, EventData payload)
        {
            var d = (EventDefinition<string, string, string?>)definition;
            var p = (BrightChainReadItemEventData)payload;
            return d.GenerateMessage(p.LogSensitiveData ? p.ResourceId : "?", p.ContainerId, p.LogSensitiveData ? p.PartitionKey : "?");
        }

        private static string FormatParameters(IReadOnlyList<(string Name, object? Value)> parameters, bool shouldLogParameterValues)
        {
            return FormatParameters(parameters.Select(p => new SqlParameter(p.Name, p.Value)).ToList(), shouldLogParameterValues);
        }

        private static string FormatParameters(IReadOnlyList<SqlParameter> parameters, bool shouldLogParameterValues)
        {
            return parameters.Count == 0
                           ? ""
                           : string.Join(", ", parameters.Select(e => FormatParameter(e, shouldLogParameterValues)));
        }

        private static string FormatParameter(SqlParameter parameter, bool shouldLogParameterValue)
        {
            var builder = new StringBuilder();
            builder
                .Append(parameter.Name)
                .Append('=');

            if (shouldLogParameterValue)
            {
                FormatParameterValue(builder, parameter.Value);
            }
            else
            {
                builder.Append('?');
            }

            return builder.ToString();
        }

        private static void FormatParameterValue(StringBuilder builder, object? parameterValue)
        {
            if (parameterValue == null)
            {
                builder.Append("null");
                return;
            }

            builder.Append('\'');

            switch (parameterValue)
            {
                case JsonElement jTokenValue:
                    builder.Append(jTokenValue.ToString().Trim('"'));
                    break;
                case DateTime dateTimeValue:
                    builder.Append(dateTimeValue.ToString("s"));
                    break;
                case DateTimeOffset dateTimeOffsetValue:
                    builder.Append(dateTimeOffsetValue.ToString("o"));
                    break;
                case byte[] binaryValue:
                    builder.AppendBytes(binaryValue);
                    break;
                default:
                    builder.Append(Convert.ToString(parameterValue, CultureInfo.InvariantCulture));
                    break;
            }

            builder.Append('\'');
        }
    }
}
