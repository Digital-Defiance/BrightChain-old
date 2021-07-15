// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace BrightChain.EntityFrameworkCore.TestUtilities
{
    public class BrightChainTestHelpers : TestHelpers
    {
        protected BrightChainTestHelpers()
        {
        }

        public static BrightChainTestHelpers Instance { get; } = new();

        public override IServiceCollection AddProviderServices(IServiceCollection services)
        {
            return services.AddEntityFrameworkBrightChain();
        }

        public override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseBrightChain(
                           TestEnvironment.DefaultConnection,
                           TestEnvironment.AuthToken,
                           "UnitTests");
        }
    }
}
