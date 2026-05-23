using MediatR;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.WasteCategories.Queries;

public class GetCategoryByIdQuery : IRequest<WasteCategoryDto?>
{
    public int Id { get; set; }
}

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, WasteCategoryDto?>
{
    private readonly IWasteCategoryRepository _repository;

    public GetCategoryByIdQueryHandler(IWasteCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<WasteCategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (category == null) return null;

        return new WasteCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description ?? string.Empty
        };
    }
}
