using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Features.ReadingResourceModule.Domain.Entities;

namespace PureTCOWebApp.Features.ReadingResourceModule.Domain.Events;

public record ReadingReportCreatedEvent(ReadingReport Report, int LastPage) : IDomainEvent;
