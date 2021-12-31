using System;
using System.Collections.Generic;
using BrightChain.Engine.Exceptions;
using BrightChain.Engine.Interfaces;
using BrightChain.Engine.Models;

namespace BrightChain.Engine.Services.CacheManagers.Block;

public abstract partial class BrightenedBlockCacheManagerBase : IBrightenedBlockCacheManager
{
    private BrightenedBlockTransaction _activeTransaction = null;

    private BrightenedBlockTransaction ActiveTransaction
    {
        get
        {
            return this._activeTransaction;
        }
    }

    public BrightenedBlockTransaction NewTransaction()
    {
        if (this._activeTransaction is not null)
        {
            throw new BrightChainException("Already in transaction");
        }

        var transaction = new BrightenedBlockTransaction(cacheManager: this);
        this._activeTransaction = transaction;
        return transaction;
    }

    public (bool Result, BrightenedBlockTransaction Transaction) Commit()
    {
        if (this._activeTransaction is null)
        {
            throw new BrightChainException("Must be in transaction");
        }

        var result = this._activeTransaction.Commit();
        var activeTransaction = this._activeTransaction;
        if (result)
        {
            this._activeTransaction = null;
        }

        return (Result: result, Transaction: activeTransaction);
    }

    public (bool Result, BrightenedBlockTransaction Transaction) Rollback()
    {
        if (this._activeTransaction is null)
        {
            throw new BrightChainException("Must be in transaction");
        }

        var result = this._activeTransaction.Rollback();
        var activeTransaction = this._activeTransaction;
        if (result)
        {
            this._activeTransaction = null;
        }

        return (Result: result, Transaction: activeTransaction);
    }
}
