// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Properties;
using BrightChain.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace BrightChain.EntityFrameworkCore.Infrastructure
{
    public class BrightChainModelValidatorTest : ModelValidatorTestBase
    {
        [ConditionalFact]
        public virtual void Passes_on_valid_model()
        {
            var modelBuilder = this.CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>();

            this.Validate(modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_on_valid_keyless_entity_type()
        {
            var modelBuilder = this.CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().HasPartitionKey(c => c.PartitionId).HasNoKey();

            var model = this.Validate(modelBuilder);

            Assert.Empty(model.FindEntityType(typeof(Customer)).GetKeys());
        }

        [ConditionalFact]
        public virtual void Detects_missing_id_property()
        {
            var modelBuilder = this.CreateConventionlessModelBuilder();
            modelBuilder.Entity<Order>(
                b =>
                {
                    b.Property(o => o.Id);
                    b.HasKey(o => o.Id);
                    b.Ignore(o => o.PartitionId);
                    b.Ignore(o => o.Customer);
                    b.Ignore(o => o.OrderDetails);
                    b.Ignore(o => o.Products);
                });

            this.VerifyError(BrightChainStrings.NoIdProperty(typeof(Order).Name), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_non_key_id_property()
        {
            var modelBuilder = this.CreateConventionlessModelBuilder();
            modelBuilder.Entity<Order>(
                b =>
                {
                    b.Property(o => o.Id);
                    b.HasKey(o => o.Id);
                    b.Property<string>("id");
                    b.Ignore(o => o.PartitionId);
                    b.Ignore(o => o.Customer);
                    b.Ignore(o => o.OrderDetails);
                    b.Ignore(o => o.Products);
                });

            this.VerifyError(BrightChainStrings.NoIdKey(typeof(Order).Name, "id"), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_non_string_id_property()
        {
            var modelBuilder = this.CreateConventionlessModelBuilder();
            modelBuilder.Entity<Order>(
                b =>
                {
                    b.Property(o => o.Id);
                    b.HasKey(o => o.Id);
                    b.Property<int>("id");
                    b.HasKey("id");
                    b.Ignore(o => o.PartitionId);
                    b.Ignore(o => o.Customer);
                    b.Ignore(o => o.OrderDetails);
                    b.Ignore(o => o.Products);
                });

            this.VerifyError(BrightChainStrings.IdNonStringStoreType("id", typeof(Order).Name, "int"), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_on_valid_partition_keys()
        {
            var modelBuilder = this.CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().ToContainer("Orders").HasPartitionKey(c => c.PartitionId);
            modelBuilder.Entity<Order>().ToContainer("Orders").HasPartitionKey(o => o.PartitionId)
                .Property(o => o.PartitionId).HasConversion<string>();

            this.Validate(modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_PK_partition_key()
        {
            var modelBuilder = this.CreateConventionalModelBuilder();
            modelBuilder.Entity<Order>(
                b =>
                {
                    b.HasKey(o => o.PartitionId);
                    b.Ignore(o => o.Customer);
                    b.Ignore(o => o.OrderDetails);
                    b.Ignore(o => o.Products);
                });

            this.Validate(modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_non_key_partition_key_property()
        {
            var modelBuilder = this.CreateConventionlessModelBuilder();
            modelBuilder.Entity<Order>(
                b =>
                {
                    b.Property(o => o.Id);
                    b.Property<string>("id");
                    b.HasKey("id");
                    b.Property(o => o.PartitionId);
                    b.HasPartitionKey(o => o.PartitionId);
                    b.Ignore(o => o.Customer);
                    b.Ignore(o => o.OrderDetails);
                    b.Ignore(o => o.Products);
                });

            this.VerifyError(BrightChainStrings.NoPartitionKeyKey(typeof(Order).Name, nameof(Order.PartitionId), "id"), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_missing_partition_key_property()
        {
            var modelBuilder = this.CreateConventionalModelBuilder();
            modelBuilder.Entity<Order>().HasPartitionKey("PartitionKey");

            this.VerifyError(BrightChainStrings.PartitionKeyMissingProperty(typeof(Order).Name, "PartitionKey"), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_missing_partition_key_on_first_type()
        {
            var modelBuilder = this.CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().ToContainer("Orders");
            modelBuilder.Entity<Order>().ToContainer("Orders").HasPartitionKey(c => c.PartitionId);

            this.VerifyError(BrightChainStrings.NoPartitionKey(typeof(Customer).Name, "Orders"), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_missing_partition_keys_one_last_type()
        {
            var modelBuilder = this.CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().ToContainer("Orders").HasPartitionKey(c => c.PartitionId);
            modelBuilder.Entity<Order>().ToContainer("Orders");

            this.VerifyError(BrightChainStrings.NoPartitionKey(typeof(Order).Name, "Orders"), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_partition_keys_mapped_to_different_properties()
        {
            var modelBuilder = this.CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().ToContainer("Orders").HasPartitionKey(c => c.PartitionId)
                .Property(c => c.PartitionId).ToJsonProperty("pk");
            modelBuilder.Entity<Order>().ToContainer("Orders").HasPartitionKey(c => c.PartitionId);

            this.VerifyError(
                BrightChainStrings.PartitionKeyStoreNameMismatch(
                    nameof(Customer.PartitionId), typeof(Customer).Name, "pk", nameof(Order.PartitionId), typeof(Order).Name,
                    nameof(Order.PartitionId)), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_partition_key_of_different_type()
        {
            var modelBuilder = this.CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().ToContainer("Orders").HasPartitionKey(c => c.PartitionId);
            modelBuilder.Entity<Order>().ToContainer("Orders").HasPartitionKey(o => o.PartitionId)
                .Property(c => c.PartitionId).HasConversion<int>();

            this.VerifyError(
                BrightChainStrings.PartitionKeyNonStringStoreType(
                    nameof(Customer.PartitionId), typeof(Order).Name, "int"), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_properties_mapped_to_same_property()
        {
            var modelBuilder = this.CreateConventionalModelBuilder();
            modelBuilder.Entity<Order>(
                ob =>
                {
                    ob.Property(o => o.Id).ToJsonProperty("Details");
                    ob.Property(o => o.PartitionId).ToJsonProperty("Details");
                });

            this.VerifyError(
                BrightChainStrings.JsonPropertyCollision(
                    nameof(Order.PartitionId), nameof(Order.Id), typeof(Order).Name, "Details"), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_property_and_embedded_type_mapped_to_same_property()
        {
            var modelBuilder = this.CreateConventionalModelBuilder();
            modelBuilder.Entity<Order>(
                ob =>
                {
                    ob.Property(o => o.PartitionId).ToJsonProperty("Details");
                    ob.OwnsOne(o => o.OrderDetails).ToJsonProperty("Details");
                });

            this.VerifyError(
                BrightChainStrings.JsonPropertyCollision(
                    nameof(Order.OrderDetails), nameof(Order.PartitionId), typeof(Order).Name, "Details"), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_missing_discriminator()
        {
            var modelBuilder = this.CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().ToContainer("Orders").HasNoDiscriminator();
            modelBuilder.Entity<Order>().ToContainer("Orders");

            this.VerifyError(BrightChainStrings.NoDiscriminatorProperty(typeof(Customer).Name, "Orders"), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_missing_discriminator_value()
        {
            var modelBuilder = this.CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().ToContainer("Orders").HasDiscriminator().HasValue(null);
            modelBuilder.Entity<Order>().ToContainer("Orders");

            this.VerifyError(BrightChainStrings.NoDiscriminatorValue(typeof(Customer).Name, "Orders"), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_discriminator_values()
        {
            var modelBuilder = this.CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>().ToContainer("Orders").HasDiscriminator().HasValue("type");
            modelBuilder.Entity<Order>().ToContainer("Orders").HasDiscriminator().HasValue("type");

            this.VerifyError(BrightChainStrings.DuplicateDiscriminatorValue(typeof(Order).Name, "type", typeof(Customer).Name, "Orders"), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Passes_on_valid_concurrency_token()
        {
            var modelBuilder = this.CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>()
                .ToContainer("Orders")
                .Property<string>("_etag")
                .IsConcurrencyToken();

            this.Validate(modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_invalid_concurrency_token()
        {
            var modelBuilder = this.CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>()
                .ToContainer("Orders")
                .Property<string>("_not_etag")
                .IsConcurrencyToken();

            this.VerifyError(BrightChainStrings.NonETagConcurrencyToken(typeof(Customer).Name, "_not_etag"), modelBuilder);
        }

        [ConditionalFact]
        public virtual void Detects_nonString_concurrency_token()
        {
            var modelBuilder = this.CreateConventionalModelBuilder();
            modelBuilder.Entity<Customer>()
                .ToContainer("Orders")
                .Property<int>("_etag")
                .IsConcurrencyToken();

            this.VerifyError(BrightChainStrings.ETagNonStringStoreType("_etag", typeof(Customer).Name, "int"), modelBuilder);
        }

        protected override TestHelpers TestHelpers
            => BrightChainTestHelpers.Instance;
    }
}
