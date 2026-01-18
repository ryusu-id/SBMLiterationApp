using Microsoft.EntityFrameworkCore;
using Npgsql;
using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Data;

public class UnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private int _transactionCount;

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _transactionCount = 0;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transactionCount == 0)
        {
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        }
        _transactionCount++;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transactionCount == 0)
        {
            throw new InvalidOperationException("No active transaction to commit");
        }

        _transactionCount--;
        
        if (_transactionCount == 0)
        {
            await _dbContext.Database.CommitTransactionAsync(cancellationToken);
        }
    }

    public async Task RollBackTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transactionCount == 0)
        {
            return;
        }

        _transactionCount = 0; // Reset count since we're rolling back everything
        await _dbContext.Database.RollbackTransactionAsync(cancellationToken);
    }

    public async Task<Result> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await BeginTransactionAsync(cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitTransactionAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateException e)
        {
            await RollBackTransactionAsync(cancellationToken);
            if (e.InnerException is NpgsqlException npgsqlException)
            {
                // PostgreSQL unique constraint violation error code is 23505
                if (npgsqlException.SqlState == "23505")
					return Result.Failure(DataDomainError.DuplicateEntry);
            }

            return Result
                .Failure(new Error("DbUpdateException", e.InnerException?.Message ?? e.Message));
        }
        catch (Exception e)
        {
            await RollBackTransactionAsync(cancellationToken);
            return Result
                .Failure(new Error("DbSaveException", e.InnerException?.Message ?? e.Message));
        }
    }
}
