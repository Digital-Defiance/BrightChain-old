// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using BrightChain.EntityFrameworkCore.Diagnostics;
using BrightChain.EntityFrameworkCore.Diagnostics.Internal;
using BrightChain.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

// ReSharper disable InconsistentNaming
namespace BrightChain.EntityFrameworkCore
{
    public class BrightChainEventIdTest : EventIdTestBase
    {
        [ConditionalFact]
        public void Every_eventId_has_a_logger_method_and_logs_when_level_enabled()
        {
            var fakeFactories = new Dictionary<Type, Func<object>>
            {
                { typeof(BrightChainSqlQuery), () => new BrightChainSqlQuery(
                    "Some SQL...",
                    new[] { new SqlParameter("P1", "V1"), new SqlParameter("P2", "V2") }) },
                { typeof(string), () => "Fake" }
            };

            this.TestEventLogging(
                typeof(BrightChainEventId),
                typeof(BrightChainLoggerExtensions),
                new BrightChainLoggingDefinitions(),
                fakeFactories);
        }
    }
}
