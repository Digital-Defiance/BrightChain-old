// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using BrightChain.EntityFrameworkCore.Infrastructure;

namespace BrightChain.EntityFrameworkCore.TestUtilities
{
    public static class BrightChainDbContextOptionsBuilderExtensions
    {
        public static BrightChainDbContextOptionsBuilder ApplyConfiguration(this BrightChainDbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ExecutionStrategy(d => new TestBrightChainExecutionStrategy(d));
            optionsBuilder.RequestTimeout(TimeSpan.FromMinutes(1));

            return optionsBuilder;
        }
    }
}
