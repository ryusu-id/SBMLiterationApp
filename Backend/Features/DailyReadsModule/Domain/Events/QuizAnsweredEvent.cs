using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Features.DailyReadsModule.Domain.Entities;

namespace PureTCOWebApp.Features.DailyReadsModule.Domain.Events;

public record QuizAnsweredEvent(DailyRead DailyRead, int UserId) : IDomainEvent;
