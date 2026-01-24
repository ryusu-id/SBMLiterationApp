using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Features.ReadingResourceModule.Domain.Entities;

namespace PureTCOWebApp.Features.ReadingResourceModule.Domain.Events;

public class ReadingReportCreatedEvent(ReadingReport Report) : IDomainEvent
{
}