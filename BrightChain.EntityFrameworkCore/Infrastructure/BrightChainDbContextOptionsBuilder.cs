// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Helpers;
using BrightChain.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel;

namespace BrightChain.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Allows in-memory specific configuration to be performed on <see cref="DbContextOptions" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from a call to
    ///         <see
    ///             cref="BrightChainDbContextOptionsExtensions.UseBrightChainDatabase(DbContextOptionsBuilder, string, System.Action{BrightChainDbContextOptionsBuilder})" />
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class BrightChainDbContextOptionsBuilder : IBrightChainDbContextOptionsBuilderInfrastructure
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BrightChainDbContextOptionsBuilder" /> class.
        /// </summary>
        /// <param name="optionsBuilder"> The options builder. </param>
        public BrightChainDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            this.OptionsBuilder = optionsBuilder;
        }

        /// <summary>
        ///     Clones the configuration in this builder.
        /// </summary>
        /// <returns> The cloned configuration. </returns>
        protected virtual DbContextOptionsBuilder OptionsBuilder { get; }

        /// <inheritdoc />
        DbContextOptionsBuilder IBrightChainDbContextOptionsBuilderInfrastructure.OptionsBuilder
            => this.OptionsBuilder;

        /// <summary>
        ///     <para>
        ///         Enables nullability check for all properties across all entities within the in-memory database.
        ///     </para>
        /// </summary>
        /// <param name="nullabilityCheckEnabled"> If <see langword="true" />, then nullability check is enforced. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual BrightChainDbContextOptionsBuilder EnableNullabilityCheck(bool nullabilityCheckEnabled = true)
        {
            var extension = this.OptionsBuilder.Options.FindExtension<BrightChainOptionsExtension>()
                ?? new BrightChainOptionsExtension();

            extension = extension.WithNullabilityCheckEnabled(nullabilityCheckEnabled);

            ((IDbContextOptionsBuilderInfrastructure)this.OptionsBuilder).AddOrUpdateExtension(extension);

            return this;
        }

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string? ToString()
            => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object? obj)
            => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
            => base.GetHashCode();

        #endregion
    }
}