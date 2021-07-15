// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using BrightChain.EntityFrameworkCore.Properties;
using BrightChain.EntityFrameworkCore.Storage.Internal;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BrightChain.EntityFrameworkCore.Infrastructure
{
    public class BrightChainTransactionManagerTest
    {
        [ConditionalFact]
        public virtual async Task BrightChainTransactionManager_does_not_support_transactions()
        {
            var transactionManager = new BrightChainTransactionManager();

            Assert.Equal(
                BrightChainStrings.TransactionsNotSupported,
                Assert.Throws<NotSupportedException>(() => transactionManager.BeginTransaction()).Message);

            Assert.Equal(
                BrightChainStrings.TransactionsNotSupported,
                (await Assert.ThrowsAsync<NotSupportedException>(async () => await transactionManager.BeginTransactionAsync())).Message);

            Assert.Equal(
                BrightChainStrings.TransactionsNotSupported,
                Assert.Throws<NotSupportedException>(() => transactionManager.CommitTransaction()).Message);

            Assert.Equal(
                BrightChainStrings.TransactionsNotSupported,
                (await Assert.ThrowsAsync<NotSupportedException>(async () => await transactionManager.CommitTransactionAsync())).Message);

            Assert.Equal(
                BrightChainStrings.TransactionsNotSupported,
                Assert.Throws<NotSupportedException>(() => transactionManager.RollbackTransaction()).Message);

            Assert.Equal(
                BrightChainStrings.TransactionsNotSupported,
                (await Assert.ThrowsAsync<NotSupportedException>(async () => await transactionManager.RollbackTransactionAsync())).Message);

            Assert.Null(transactionManager.CurrentTransaction);
            Assert.Null(transactionManager.EnlistedTransaction);

            Assert.Equal(
                BrightChainStrings.TransactionsNotSupported,
                Assert.Throws<NotSupportedException>(() => transactionManager.EnlistTransaction(null)).Message);

            transactionManager.ResetState();
            await transactionManager.ResetStateAsync();

            Assert.Null(transactionManager.CurrentTransaction);
            Assert.Null(transactionManager.EnlistedTransaction);
        }
    }
}
