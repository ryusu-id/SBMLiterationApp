using PureTCOWebApp.Core.Events;
using PureTCOWebApp.Features.ReadingResourceModule.Domain.Entities;

namespace PureTCOWebApp.Features.ReadingResourceModule.Domain.Events;

public record BookCompletedEvent(ReadingReport Report) : IDomainEvent;
