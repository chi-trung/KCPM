using System.Security.Claims;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WastePlatform.API.Controllers;
using WastePlatform.Application.Complaints.Commands;
using WastePlatform.Application.Complaints.Queries;
using WastePlatform.Application.Common.DTOs;
using WastePlatform.Domain.Enums;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Controllers;

[AllureEpic("Complaint APIs")]
[AllureFeature("Complaints Controller")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Create, list, view, and escalate complaints")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "ComplaintsControllerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("Nguyễn Chí Trung")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("api")]
[Allure.Net.Commons.Attributes.AllureTag("complaints")]
public class ComplaintsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();

    [Fact]
    [AllureDescription("CreateComplaint returns Created with complaint data for valid input.")]
    public async Task CreateComplaint_WithValidInput_ShouldReturnCreated()
    {
        var userId = Guid.NewGuid();
        var complaintId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateComplaintCommand>(), default))
            .ReturnsAsync(complaintId);

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetComplaintByIdQuery>(q => q.Id == complaintId), default))
            .ReturnsAsync(new ComplaintDto
            {
                Id = complaintId,
                CitizenId = userId,
                Content = "Test complaint",
                Status = ComplaintStatus.Open
            });

        var controller = CreateController(userId);
        var dto = new CreateComplaintDto { Content = "Test complaint" };

        var result = await controller.CreateComplaint(dto);

        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        AllureAttachmentHelper.AttachJson("create-complaint-response", createdResult.Value!);
    }

    [Fact]
    [AllureDescription("CreateComplaint returns BadRequest when content is empty.")]
    public async Task CreateComplaint_WithEmptyContent_ShouldReturnBadRequest()
    {
        AllureAttachmentHelper.AttachText("test-c-r-e-a-t-e-c-o-m-p-l-a-i-n-t_-w-i-t-h-e-m-p-t-y-c", "Executed: CreateComplaint_WithEmptyContent_ShouldReturnBadRequest");
        var controller = CreateController(Guid.NewGuid());
        var dto = new CreateComplaintDto { Content = "  " };

        var result = await controller.CreateComplaint(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("CreateComplaint returns Unauthorized when user ID is missing.")]
    public async Task CreateComplaint_WhenNoAuth_ShouldReturnUnauthorized()
    {
        AllureAttachmentHelper.AttachText("test-c-r-e-a-t-e-c-o-m-p-l-a-i-n-t_-w-h-e-n-n-o-a-u-t-h", "Executed: CreateComplaint_WhenNoAuth_ShouldReturnUnauthorized");
        var controller = CreateControllerWithoutAuth();
        var dto = new CreateComplaintDto { Content = "Valid content" };

        var result = await controller.CreateComplaint(dto);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    [AllureDescription("GetComplaints returns OK with paginated complaints list.")]
    public async Task GetComplaints_WithValidParams_ShouldReturnOk()
    {
        AllureAttachmentHelper.AttachText("test-g-e-t-c-o-m-p-l-a-i-n-t-s_-w-i-t-h-v-a-l-i-d-p-a-r", "Executed: GetComplaints_WithValidParams_ShouldReturnOk");
        var userId = Guid.NewGuid();
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetCitizenComplaintsQuery>(), default))
            .ReturnsAsync(new ComplaintsResponseDto());

        var controller = CreateController(userId);

        var result = await controller.GetComplaints(page: 1, pageSize: 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    [AllureDescription("GetComplaints returns BadRequest when page is less than 1.")]
    public async Task GetComplaints_WithInvalidPagination_ShouldReturnBadRequest()
    {
        AllureAttachmentHelper.AttachText("test-g-e-t-c-o-m-p-l-a-i-n-t-s_-w-i-t-h-i-n-v-a-l-i-d-p", "Executed: GetComplaints_WithInvalidPagination_ShouldReturnBadRequest");
        var controller = CreateController(Guid.NewGuid());

        var result = await controller.GetComplaints(page: 0, pageSize: 10);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("GetComplaints returns Unauthorized when user ID is missing.")]
    public async Task GetComplaints_WhenNoAuth_ShouldReturnUnauthorized()
    {
        AllureAttachmentHelper.AttachText("test-g-e-t-c-o-m-p-l-a-i-n-t-s_-w-h-e-n-n-o-a-u-t-h_-s-", "Executed: GetComplaints_WhenNoAuth_ShouldReturnUnauthorized");
        var controller = CreateControllerWithoutAuth();

        var result = await controller.GetComplaints();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    [AllureDescription("GetComplaintDetail returns OK for complaint owned by user.")]
    public async Task GetComplaintDetail_WhenOwned_ShouldReturnOk()
    {
        AllureAttachmentHelper.AttachText("test-g-e-t-c-o-m-p-l-a-i-n-t-d-e-t-a-i-l_-w-h-e-n-o-w-n", "Executed: GetComplaintDetail_WhenOwned_ShouldReturnOk");
        var userId = Guid.NewGuid();
        var complaintId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.Is<GetComplaintByIdQuery>(q => q.Id == complaintId), default))
            .ReturnsAsync(new ComplaintDto
            {
                Id = complaintId,
                CitizenId = userId,
                Content = "My complaint",
                Status = ComplaintStatus.Open
            });

        var controller = CreateController(userId);

        var result = await controller.GetComplaintDetail(complaintId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    [AllureDescription("GetComplaintDetail returns NotFound when complaint doesn't exist.")]
    public async Task GetComplaintDetail_WhenNotFound_ShouldReturnNotFound()
    {
        AllureAttachmentHelper.AttachText("test-g-e-t-c-o-m-p-l-a-i-n-t-d-e-t-a-i-l_-w-h-e-n-n-o-t", "Executed: GetComplaintDetail_WhenNotFound_ShouldReturnNotFound");
        var userId = Guid.NewGuid();
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetComplaintByIdQuery>(), default))
            .ReturnsAsync((ComplaintDto?)null);

        var controller = CreateController(userId);

        var result = await controller.GetComplaintDetail(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    [AllureDescription("GetComplaintDetail returns Forbid when complaint belongs to another user.")]
    public async Task GetComplaintDetail_WhenNotOwned_ShouldReturnForbid()
    {
        AllureAttachmentHelper.AttachText("test-g-e-t-c-o-m-p-l-a-i-n-t-d-e-t-a-i-l_-w-h-e-n-n-o-t", "Executed: GetComplaintDetail_WhenNotOwned_ShouldReturnForbid");
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetComplaintByIdQuery>(), default))
            .ReturnsAsync(new ComplaintDto
            {
                Id = Guid.NewGuid(),
                CitizenId = otherUserId,  // Different user
                Content = "Other user's complaint",
                Status = ComplaintStatus.Open
            });

        var controller = CreateController(userId);

        var result = await controller.GetComplaintDetail(Guid.NewGuid());

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    [AllureDescription("EscalateToAdmin returns Ok when escalation succeeds.")]
    public async Task EscalateToAdmin_WhenSuccessful_ShouldReturnOk()
    {
        var userId = Guid.NewGuid();
        var complaintId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CitizenEscalateComplaintCommand>(), default))
            .ReturnsAsync(new EscalateComplaintResult
            {
                Success = true,
                Message = "Escalated successfully",
                ComplaintId = complaintId,
                NewStatus = "EscalatedToAdmin"
            });

        var controller = CreateController(userId);

        var result = await controller.EscalateToAdmin(complaintId, new CitizenEscalateRequest { Reason = "Not resolved" });

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    [AllureDescription("EscalateToAdmin returns BadRequest when escalation fails.")]
    public async Task EscalateToAdmin_WhenFails_ShouldReturnBadRequest()
    {
        var userId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CitizenEscalateComplaintCommand>(), default))
            .ReturnsAsync(new EscalateComplaintResult
            {
                Success = false,
                Message = "Cannot escalate at this stage"
            });

        var controller = CreateController(userId);

        var result = await controller.EscalateToAdmin(Guid.NewGuid(), new CitizenEscalateRequest());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("EscalateToAdmin returns Unauthorized when user ID is missing.")]
    public async Task EscalateToAdmin_WhenNoAuth_ShouldReturnUnauthorized()
    {
        var controller = CreateControllerWithoutAuth();

        var result = await controller.EscalateToAdmin(Guid.NewGuid(), new CitizenEscalateRequest());

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    [AllureDescription("EscalateToAdmin returns 500 when mediator throws an exception.")]
    public async Task EscalateToAdmin_WhenException_ShouldReturn500()
    {
        var userId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CitizenEscalateComplaintCommand>(), default))
            .ThrowsAsync(new Exception("Database failure"));

        var controller = CreateController(userId);

        var result = await controller.EscalateToAdmin(Guid.NewGuid(), new CitizenEscalateRequest());

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    private ComplaintsController CreateController(Guid userId)
    {
        var controller = new ComplaintsController(_mediatorMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Email, "citizen@test.com"),
                    new Claim(ClaimTypes.Role, "Citizen")
                ], "TestAuth"))
            }
        };
        return controller;
    }

    private ComplaintsController CreateControllerWithoutAuth()
    {
        var controller = new ComplaintsController(_mediatorMock.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }
}
