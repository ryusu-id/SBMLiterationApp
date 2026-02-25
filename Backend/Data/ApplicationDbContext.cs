using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Features.Auth.Domain;
using PureTCOWebApp.Features.TestModule;
using PureTCOWebApp.Features.ReadingResourceModule.Domain;
using PureTCOWebApp.Features.ReadingResourceModule.Domain.Entities;
using PureTCOWebApp.Features.ReadingCategoryModule.Domain;
using PureTCOWebApp.Features.ReadingRecommendationModule.Domain;
using PureTCOWebApp.Features.StreakModule.Domain;
using PureTCOWebApp.Features.DailyReadsModule.Domain;
using PureTCOWebApp.Features.DailyReadsModule.Domain.Entities;
using PureTCOWebApp.Features.UserXpModule.Domain;
using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Features.GroupModule.Domain;
using PureTCOWebApp.Features.AssignmentModule.Domain;

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
    public DbSet<StreakLog> StreakLogs { get; set; }
    public DbSet<ReadingCategory> ReadingCategories { get; set; }
    public DbSet<ReadingRecommendation> ReadingRecommendations { get; set; }
    public DbSet<DailyRead> DailyReads { get; set; }
    public DbSet<QuizQuestion> QuizQuestions { get; set; }
    public DbSet<QuizChoice> QuizChoices { get; set; }
    public DbSet<QuizAnswer> QuizAnswers { get; set; }
    public DbSet<UserExpEvent> UserExpEvents { get; set; }
    public DbSet<UserExpSnapshot> UserExpSnapshots { get; set; }
    public DbSet<UserExpLeaderboard> UserExpLeaderboard { get; set; }

    // Group module
    public DbSet<Group> Groups { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }

    // Assignment module
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
    public DbSet<AssignmentSubmissionFile> AssignmentSubmissionFiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
