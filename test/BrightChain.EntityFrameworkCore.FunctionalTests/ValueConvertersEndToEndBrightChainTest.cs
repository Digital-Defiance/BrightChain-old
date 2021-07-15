// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

#nullable enable

namespace BrightChain.EntityFrameworkCore
{
    public class ValueConvertersEndToEndBrightChainTest
        : ValueConvertersEndToEndTestBase<ValueConvertersEndToEndBrightChainTest.ValueConvertersEndToEndBrightChainFixture>
    {
        public ValueConvertersEndToEndBrightChainTest(ValueConvertersEndToEndBrightChainFixture fixture)
            : base(fixture)
        {
        }

        public class ValueConvertersEndToEndBrightChainFixture : ValueConvertersEndToEndFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => BrightChainTestStoreFactory.Instance;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<ConvertingEntity>(
                    b =>
                    {
                        // Issue #24684
                        b.Ignore(e => e.StringToDateTimeOffset);
                        b.Ignore(e => e.NullableStringToDateTimeOffset);
                        b.Ignore(e => e.StringToNullableDateTimeOffset);
                        b.Ignore(e => e.NullableStringToNullableDateTimeOffset);
                    });
            }
        }
    }
}

#nullable restore
