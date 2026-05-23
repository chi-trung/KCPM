using MediatR;
using WastePlatform.Application.Admin.Users.DTOs;
using WastePlatform.Application.Common.Interfaces; // Đã thêm để dùng IUserRepository

namespace WastePlatform.Application.Admin.Users.Queries
{
    // Yêu cầu lấy danh sách người dùng, nhận vào tham số từ Frontend
    public class GetUsersQuery : IRequest<List<UserDto>>
    {
        public string? Search { get; set; }
        public string? Role { get; set; }
    }

    // Handler xử lý logic tìm kiếm và lọc
    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
    {
        private readonly IUserRepository _userRepository;

        // Inject Interface UserRepository vào đây
        public GetUsersQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            // 1. Gọi xuống Database qua Repository để lấy dữ liệu thật
            var usersFromDb = await _userRepository.GetUsersAsync(request.Search, request.Role, cancellationToken);

            // 2. Map (Chuyển đổi) từ Entity (User) sang DTO (UserDto) để trả về cho Frontend
            var userDtos = usersFromDb.Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role.ToString().ToLower(),
                IsActive = u.IsActive,
                // Tạm thời lấy giờ hiện tại, nếu Entity User của bạn có trường CreatedAt/UpdatedAt thì thay vào nhé
                LastActiveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm") 
            }).ToList();

            return userDtos;
        }
    }
}