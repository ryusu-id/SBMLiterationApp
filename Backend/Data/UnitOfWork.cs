using Microsoft.EntityFrameworkCore;
using Npgsql;
using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Data;

public class UnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private int _transactionCount;
    private readonly DomainEventsDispatcher domainEventsDispatcher;

    public UnitOfWork(ApplicationDbContext dbContext, DomainEventsDispatcher domainEventsDispatcher)
    {
        _dbContext = dbContext;
        this.domainEventsDispatcher = domainEventsDispatcher;
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
            await PublishDomainEventsAsync();
            
            // Save changes made by event handlers
            var hasChanges = _dbContext.ChangeTracker.Entries()
                .Any(e => e.State == EntityState.Added || 
                         e.State == EntityState.Modified || 
                         e.State == EntityState.Deleted);
            
            if (hasChanges)
            {
                await SaveChangesAsync(cancellationToken);
            }
            
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
    private async Task PublishDomainEventsAsync()
    {
        var domainEvents = _dbContext.ChangeTracker
            .Entries<AuditableEntity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                List<IDomainEvent> domainEvents = entity.DomainEvents;

                entity.ClearDomainEvents();

                return domainEvents;
            })
            .ToList();

        await domainEventsDispatcher.DispatchAsync(domainEvents);
    }
}
