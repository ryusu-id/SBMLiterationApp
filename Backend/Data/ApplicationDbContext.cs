using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Features.Auth.Domain;
using PureTCOWebApp.Features.TestModule;
using PureTCOWebApp.Features.ReadingResourceModule.Domain;
using PureTCOWebApp.Features.ReadingResourceModule.Domain.Entities;
using PureTCOWebApp.Features.ReadingCategoryModule.Domain;
using PureTCOWebApp.Features.ReadingRecommendationModule.Domain;
using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Data;
public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    private readonly DomainEventsDispatcher domainEventsDispatcher;
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        DomainEventsDispatcher domainEventsDispatcher) 
        : base(options)
    {
        this.domainEventsDispatcher = domainEventsDispatcher;
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<TestItem> TestItems { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<JournalPaper> JournalPapers { get; set; }
    public DbSet<ReadingReport> ReadingReports { get; set; }
    public DbSet<StreakExp> StreakExps { get; set; }
    public DbSet<ReadingCategory> ReadingCategories { get; set; }
    public DbSet<ReadingRecommendation> ReadingRecommendations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // When should you publish domain events?
        //
        // 1. BEFORE calling SaveChangesAsync
        //     - domain events are part of the same transaction
        //     - immediate consistency
        // 2. AFTER calling SaveChangesAsync
        //     - domain events are a separate transaction
        //     - eventual consistency
        //     - handlers can fail

        int result = await base.SaveChangesAsync(cancellationToken);

        await PublishDomainEventsAsync();

        return result;
    }

    private async Task PublishDomainEventsAsync()
    {
        var domainEvents = ChangeTracker
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