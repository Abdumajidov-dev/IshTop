using IshTop.Application.Common.Models;
using IshTop.Domain.Entities;
using IshTop.Domain.Enums;
using IshTop.Domain.Interfaces.Repositories;
using MediatR;

namespace IshTop.Application.Users.Commands.CreateUser;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Users.GetByTelegramIdAsync(request.TelegramId, cancellationToken);
        if (existing is not null)
            return Result<Guid>.Success(existing.Id);

        var user = new User
        {
            TelegramId = request.TelegramId,
            Username = request.Username,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Language = request.Language,
            State = UserState.New,
            Profile = new UserProfile()
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(user.Id);
    }
}
