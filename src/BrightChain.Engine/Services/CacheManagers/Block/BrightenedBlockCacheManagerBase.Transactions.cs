using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models;

namespace BrightChain.Engine.Services.CacheManagers.Block;

public abstract partial class BrightenedBlockCacheManagerBase : IBrightenedBlockCacheManager
{
    private BrightenedBlockTransaction ActiveTransaction { get; set; }

    public BrightenedBlockTransaction NewTransaction()
    {
        if (this.ActiveTransaction is not null)
        {
            throw new BrightChainException(message: "Already in transaction");
        }

        var transaction = new BrightenedBlockTransaction(cacheManager: this);
        this.ActiveTransaction = transaction;
        return transaction;
    }

    public (bool Result, BrightenedBlockTransaction Transaction) Commit()
    {
        if (this.ActiveTransaction is null)
        {
            throw new BrightChainException(message: "Must be in transaction");
        }

        var result = this.ActiveTransaction.Commit();
        var activeTransaction = this.ActiveTransaction;
        if (result)
        {
            this.ActiveTransaction = null;
        }

        return (Result: result, Transaction: activeTransaction);
    }

    public (bool Result, BrightenedBlockTransaction Transaction) Rollback()
    {
        if (this.ActiveTransaction is null)
        {
            throw new BrightChainException(message: "Must be in transaction");
        }

        var result = this.ActiveTransaction.Rollback();
        var activeTransaction = this.ActiveTransaction;
        if (result)
        {
            this.ActiveTransaction = null;
        }

        return (Result: result, Transaction: activeTransaction);
    }
}
