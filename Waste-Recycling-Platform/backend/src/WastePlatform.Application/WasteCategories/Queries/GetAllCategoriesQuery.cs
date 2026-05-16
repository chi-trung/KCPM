using MediatR;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Application.WasteCategories.Queries;

public class WasteCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class GetAllCategoriesQuery : IRequest<IEnumerable<WasteCategoryDto>>
{
}

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, IEnumerable<WasteCategoryDto>>
{
    private readonly IWasteCategoryRepository _repository;

    public GetAllCategoriesQueryHandler(IWasteCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<WasteCategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _repository.GetAllAsync(cancellationToken);
        
        return categories.Select(c => new WasteCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description ?? string.Empty
        });
    }
}
