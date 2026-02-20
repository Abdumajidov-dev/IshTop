using MediatR;

namespace IshTop.Domain.Events;

public record ProfileCompletedEvent(Guid UserId) : INotification;
