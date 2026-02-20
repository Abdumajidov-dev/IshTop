using IshTop.Application.Common.Models;
using IshTop.Domain.Interfaces.Repositories;
using IshTop.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Pgvector;

namespace IshTop.Application.Jobs.Commands.CreateJob;

public class CreateJobHandler : IRequestHandler<CreateJobCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiService _aiService;
    private readonly IJobMatchingService _matchingService;
    private readonly ILogger<CreateJobHandler> _logger;

    public CreateJobHandler(IUnitOfWork unitOfWork, IAiService aiService,
        IJobMatchingService matchingService, ILogger<CreateJobHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _aiService = aiService;
        _matchingService = matchingService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateJobCommand request, CancellationToken cancellationToken)
    {
        // Check for spam
        var isSpam = await _aiService.DetectSpamAsync(request.RawText, cancellationToken);
        if (isSpam)
        {
            _logger.LogInformation("Spam detected, skipping message from channel {ChannelId}", request.SourceChannelTelegramId);
            return Result<Guid>.Failure("Spam detected");
        }

        // Check for duplicate
        var embedding = await _aiService.GenerateEmbeddingAsync(request.RawText, cancellationToken);
        var vector = new Vector(embedding);
        var isDuplicate = await _unitOfWork.Jobs.IsDuplicateAsync(vector, 0.95, cancellationToken);
        if (isDuplicate)
        {
            _logger.LogInformation("Duplicate job detected, skipping");
            return Result<Guid>.Failure("Duplicate job");
        }

        // Parse job from raw text
        var job = await _aiService.ParseJobFromMessageAsync(request.RawText, cancellationToken);
        job.Embedding = vector;
        job.SourceMessageId = request.SourceMessageId;

        // Link to channel
        var channel = await _unitOfWork.Channels.GetByTelegramIdAsync(request.SourceChannelTelegramId, cancellationToken);
        if (channel is not null)
        {
            job.SourceChannelId = channel.Id;
            channel.JobCount++;
            channel.LastParsedAt = DateTime.UtcNow;
        }

        await _unitOfWork.Jobs.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("New job created: {JobTitle} (ID: {JobId})", job.Title, job.Id);

        return Result<Guid>.Success(job.Id);
    }
}
