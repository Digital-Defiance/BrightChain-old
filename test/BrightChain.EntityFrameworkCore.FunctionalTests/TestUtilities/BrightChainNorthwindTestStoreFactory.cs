// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace BrightChain.EntityFrameworkCore.TestUtilities
{
    public class BrightChainNorthwindTestStoreFactory : BrightChainTestStoreFactory
    {
        private const string Name = "Northwind";

        public static new BrightChainNorthwindTestStoreFactory Instance { get; } = new();

        protected BrightChainNorthwindTestStoreFactory()
        {
        }

        public override TestStore GetOrCreate(string storeName)
        {
            return BrightChainTestStore.GetOrCreate(Name, "Northwind.json");
        }
    }
}
