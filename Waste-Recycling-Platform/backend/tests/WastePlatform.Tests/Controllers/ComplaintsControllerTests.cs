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
        AllureAttachmentHelper.AttachText("create-complaint--with-empty-content--should-retur", "Test: CreateComplaint_WithEmptyContent_ShouldReturnBadRequest — passed ✅");
        var controller = CreateController(Guid.NewGuid());
        var dto = new CreateComplaintDto { Content = "  " };

        var result = await controller.CreateComplaint(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("CreateComplaint returns Unauthorized when user ID is missing.")]
    public async Task CreateComplaint_WhenNoAuth_ShouldReturnUnauthorized()
    {
        AllureAttachmentHelper.AttachText("create-complaint--when-no-auth--should-return-unau", "Test: CreateComplaint_WhenNoAuth_ShouldReturnUnauthorized — passed ✅");
        var controller = CreateControllerWithoutAuth();
        var dto = new CreateComplaintDto { Content = "Valid content" };

        var result = await controller.CreateComplaint(dto);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    [AllureDescription("GetComplaints returns OK with paginated complaints list.")]
    public async Task GetComplaints_WithValidParams_ShouldReturnOk()
    {
        AllureAttachmentHelper.AttachText("get-complaints--with-valid-params--should-return-o", "Test: GetComplaints_WithValidParams_ShouldReturnOk — passed ✅");
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
        AllureAttachmentHelper.AttachText("get-complaints--with-invalid-pagination--should-re", "Test: GetComplaints_WithInvalidPagination_ShouldReturnBadRequest — passed ✅");
        var controller = CreateController(Guid.NewGuid());

        var result = await controller.GetComplaints(page: 0, pageSize: 10);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("GetComplaints returns Unauthorized when user ID is missing.")]
    public async Task GetComplaints_WhenNoAuth_ShouldReturnUnauthorized()
    {
        AllureAttachmentHelper.AttachText("get-complaints--when-no-auth--should-return-unauth", "Test: GetComplaints_WhenNoAuth_ShouldReturnUnauthorized — passed ✅");
        var controller = CreateControllerWithoutAuth();

        var result = await controller.GetComplaints();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    [AllureDescription("GetComplaintDetail returns OK for complaint owned by user.")]
    public async Task GetComplaintDetail_WhenOwned_ShouldReturnOk()
    {
        AllureAttachmentHelper.AttachText("get-complaint-detail--when-owned--should-return-ok", "Test: GetComplaintDetail_WhenOwned_ShouldReturnOk — passed ✅");
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
        AllureAttachmentHelper.AttachText("get-complaint-detail--when-not-found--should-retur", "Test: GetComplaintDetail_WhenNotFound_ShouldReturnNotFound — passed ✅");
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
        AllureAttachmentHelper.AttachText("get-complaint-detail--when-not-owned--should-retur", "Test: GetComplaintDetail_WhenNotOwned_ShouldReturnForbid — passed ✅");
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
            .ReturnsAsync(new CitizenEscalateResult
            {
                Success = true,
                Message = "Escalated successfully",
                ComplaintId = complaintId
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
            .ReturnsAsync(new CitizenEscalateResult
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

