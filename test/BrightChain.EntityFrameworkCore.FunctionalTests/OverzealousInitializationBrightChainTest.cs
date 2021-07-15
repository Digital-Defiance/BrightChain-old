// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace BrightChain.EntityFrameworkCore
{
    public class OverzealousInitializationBrightChainTest
        : OverzealousInitializationTestBase<OverzealousInitializationBrightChainTest.OverzealousInitializationBrightChainFixture>
    {
        public OverzealousInitializationBrightChainTest(OverzealousInitializationBrightChainFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "Issue #17246")]
        public override void Fixup_does_not_ignore_eagerly_initialized_reference_navs()
        {
        }

        public class OverzealousInitializationBrightChainFixture : OverzealousInitializationFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => BrightChainTestStoreFactory.Instance;
        }
    }
}
