using MediatR;
using Microsoft.AspNetCore.Http;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Entities;

namespace WastePlatform.Application.Reports.Commands;

public class CreateReportCommand : IRequest<Guid>
{
    public Guid CitizenId { get; set; }
    public int WasteCategoryId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? AiSuggestion { get; set; }
    public IFormFileCollection? Images { get; set; }
}

public class CreateReportCommandHandler : IRequestHandler<CreateReportCommand, Guid>
{
    private readonly IReportRepository _reportRepository;
    private readonly IWasteCategoryRepository _categoryRepository;
    private readonly IFileStorageService _fileStorageService;

    public CreateReportCommandHandler(
        IReportRepository reportRepository,
        IWasteCategoryRepository categoryRepository,
        IFileStorageService fileStorageService)
    {
        _reportRepository = reportRepository;
        _categoryRepository = categoryRepository;
        _fileStorageService = fileStorageService;
    }

    public async Task<Guid> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.WasteCategoryId, cancellationToken);
        if (category == null)
            throw new ArgumentException("Invalid waste category");

        if (request.Latitude < -90 || request.Latitude > 90 || request.Longitude < -180 || request.Longitude > 180)
            throw new ArgumentException("Invalid latitude or longitude coordinates");

        var report = WasteReport.Create(
            citizenId: request.CitizenId,
            wasteCategoryId: request.WasteCategoryId,
            latitude: request.Latitude,
            longitude: request.Longitude,
            description: request.Description ?? string.Empty,
            address: request.Address ?? string.Empty,
            aiSuggestion: request.AiSuggestion ?? string.Empty
        );

        if (request.Images != null && request.Images.Count > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            
            foreach (var file in request.Images)
            {
                var fileName = await _fileStorageService.SaveFileAsync(file, allowedExtensions, 5 * 1024 * 1024, cancellationToken);
                
                report.Images.Add(new ReportImage
                {
                    Id = Guid.NewGuid(),
                    ReportId = report.Id,
                    ImageUrl = fileName,
                    UploadedAt = DateTime.UtcNow
                });
            }
        }

        await _reportRepository.AddAsync(report, cancellationToken);
        await _reportRepository.SaveChangesAsync(cancellationToken);

        return report.Id;
    }
}
