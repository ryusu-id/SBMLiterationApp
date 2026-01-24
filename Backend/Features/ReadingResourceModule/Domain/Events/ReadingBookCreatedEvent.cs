using PureTCOWebApp.Core.Events;

namespace PureTCOWebApp.Features.ReadingResourceModule.Domain.Events;

public record ReadingBookCreatedEvent(Book Book) : IDomainEvent;

public class ReadingBookCreatedEventHandler : IDomainEventHandler<ReadingBookCreatedEvent>
{
    public Task Handle(ReadingBookCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        Console.WriteLine($"ReadingBookCreatedEvent handled for Book Id: {domainEvent.Book.Id}");
        return Task.CompletedTask;
    }
}