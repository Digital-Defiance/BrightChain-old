// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using BrightChain.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BrightChain.EntityFrameworkCore.TestUtilities
{
    public class BrightChainTestStoreFactory : ITestStoreFactory
    {
        public static BrightChainTestStoreFactory Instance { get; } = new();

        protected BrightChainTestStoreFactory()
        {
        }

        public IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
        {
            return serviceCollection
                           .AddEntityFrameworkBrightChain()
                           .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                           .AddSingleton<TestStoreIndex>();
        }

        public TestStore Create(string storeName)
        {
            return BrightChainTestStore.Create(storeName);
        }

        public virtual TestStore GetOrCreate(string storeName)
        {
            return BrightChainTestStore.GetOrCreate(storeName);
        }

        public virtual ListLoggerFactory CreateListLoggerFactory(Func<string, bool> shouldLogCategory)
        {
            return new TestSqlLoggerFactory(shouldLogCategory);
        }
    }
}
