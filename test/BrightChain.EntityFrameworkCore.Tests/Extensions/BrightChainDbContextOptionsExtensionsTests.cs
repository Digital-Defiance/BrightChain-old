// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using BrightChain.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;
using static Microsoft.EntityFrameworkCore.InMemoryDbContextOptionsExtensions;

// ReSharper disable once CheckNamespace
namespace BrightChain.EntityFrameworkCore
{
    public class BrightChainDbContextOptionsExtensionsTests
    {
        [ConditionalFact]
        public void Throws_with_multiple_providers_new_when_no_provider()
        {
            var options = new DbContextOptionsBuilder()
                .UseBrightChain("serviceEndPoint", "authKeyOrResourceToken", "databaseName")
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new DbContext(options);

            Assert.Equal(
                CoreStrings.MultipleProvidersConfigured("'BrightChain.EntityFrameworkCore', 'Microsoft.EntityFrameworkCore.InMemory'"),
                Assert.Throws<InvalidOperationException>(() => context.Model).Message);
        }

        [ConditionalFact]
        public void Can_create_options_with_wrong_region()
        {
            var regionName = "FakeRegion";
            var options = new DbContextOptionsBuilder().UseBrightChain(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.Region(regionName); });

            var extension = options
                .Options.FindExtension<BrightChainOptionsExtension>();

            // The region will be validated by the BrightChain SDK, because the region list is not constant
            Assert.Equal(regionName, extension.Region);
        }

        [ConditionalFact]
        public void Can_create_options_and_limit_to_endpoint()
        {
            var options = new DbContextOptionsBuilder().UseBrightChain(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.LimitToEndpoint(); });

            var extension = options.Options.FindExtension<BrightChainOptionsExtension>();

            Assert.True(extension.LimitToEndpoint);
        }

        [ConditionalFact]
        public void Can_create_options_with_web_proxy()
        {
            var webProxy = new WebProxy();
            var options = new DbContextOptionsBuilder().UseBrightChain(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.WebProxy(webProxy); });

            var extension = options.Options.FindExtension<BrightChainOptionsExtension>();

            Assert.Same(webProxy, extension.WebProxy);
        }

        [ConditionalFact]
        public void Can_create_options_with_request_timeout()
        {
            var requestTimeout = TimeSpan.FromMinutes(3);
            var options = new DbContextOptionsBuilder().UseBrightChain(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.RequestTimeout(requestTimeout); });

            var extension = options.Options.FindExtension<BrightChainOptionsExtension>();

            Assert.Equal(requestTimeout, extension.RequestTimeout);
        }

        [ConditionalFact]
        public void Can_create_options_with_open_tcp_connection_timeout()
        {
            var timeout = TimeSpan.FromMinutes(3);
            var options = new DbContextOptionsBuilder().UseBrightChain(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.OpenTcpConnectionTimeout(timeout); });

            var extension = options.Options.FindExtension<BrightChainOptionsExtension>();

            Assert.Equal(timeout, extension.OpenTcpConnectionTimeout);
        }

        [ConditionalFact]
        public void Can_create_options_with_idle_tcp_connection_timeout()
        {
            var timeout = TimeSpan.FromMinutes(3);
            var options = new DbContextOptionsBuilder().UseBrightChain(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.IdleTcpConnectionTimeout(timeout); });

            var extension = options.Options.FindExtension<BrightChainOptionsExtension>();

            Assert.Equal(timeout, extension.IdleTcpConnectionTimeout);
        }

        [ConditionalFact]
        public void Can_create_options_with_gateway_mode_max_connection_limit()
        {
            var connectionLimit = 3;
            var options = new DbContextOptionsBuilder().UseBrightChain(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.GatewayModeMaxConnectionLimit(connectionLimit); });

            var extension = options.Options.FindExtension<BrightChainOptionsExtension>();

            Assert.Equal(connectionLimit, extension.GatewayModeMaxConnectionLimit);
        }

        [ConditionalFact]
        public void Can_create_options_with_max_tcp_connections_per_endpoint()
        {
            var connectionLimit = 3;
            var options = new DbContextOptionsBuilder().UseBrightChain(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.MaxTcpConnectionsPerEndpoint(connectionLimit); });

            var extension = options.Options.FindExtension<BrightChainOptionsExtension>();

            Assert.Equal(connectionLimit, extension.MaxTcpConnectionsPerEndpoint);
        }

        [ConditionalFact]
        public void Can_create_options_with_max_requests_per_tcp_connection()
        {
            var requestLimit = 3;
            var options = new DbContextOptionsBuilder().UseBrightChain(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.MaxRequestsPerTcpConnection(requestLimit); });

            var extension = options.Options.FindExtension<BrightChainOptionsExtension>();

            Assert.Equal(requestLimit, extension.MaxRequestsPerTcpConnection);
        }

        [ConditionalFact]
        public void Can_create_options_with_content_response_on_write_enabled()
        {
            var enabled = true;
            var options = new DbContextOptionsBuilder().UseBrightChain(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.ContentResponseOnWriteEnabled(enabled); });

            var extension = options.Options.FindExtension<BrightChainOptionsExtension>();

            Assert.Equal(enabled, extension.EnableContentResponseOnWrite);
        }
    }
}

