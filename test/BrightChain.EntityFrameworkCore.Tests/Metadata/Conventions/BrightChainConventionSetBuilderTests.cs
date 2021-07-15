// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Linq;
using Xunit;

namespace BrightChain.EntityFrameworkCore.Metadata.Conventions
{
    public class BrightChainConventionSetBuilderTests : ConventionSetBuilderTests
    {
        public override IReadOnlyModel Can_build_a_model_with_default_conventions_without_DI()
        {
            return null;
        }

        public override IReadOnlyModel Can_build_a_model_with_default_conventions_without_DI_new()
        {
            var model = base.Can_build_a_model_with_default_conventions_without_DI_new();

            Assert.Equal("DbContext", model.GetEntityTypes().Single().GetContainer());

            return model;
        }

        protected override ModelBuilder GetModelBuilder()
        {
            return BrightChainConventionSetBuilder.CreateModelBuilder();
        }
    }
}
