// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.TestUtilities;
using Xunit;

// Skip the entire assembly if cannot connect to BrightChainDb
[assembly: BrightChainDbConfiguredCondition]

// Waiting on Task causes deadlocks when run in parallel
[assembly: CollectionBehavior(DisableTestParallelization = true)]
