using MediatR;

namespace IshTop.Domain.Events;

public record JobCreatedEvent(Guid JobId) : INotification;
