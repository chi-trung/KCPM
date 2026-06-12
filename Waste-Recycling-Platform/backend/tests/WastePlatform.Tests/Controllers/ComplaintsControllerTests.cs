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
[AllureOwner("Chi Trung")]
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
        var controller = CreateController(Guid.NewGuid());
        var dto = new CreateComplaintDto { Content = "  " };

        var result = await controller.CreateComplaint(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("CreateComplaint returns Unauthorized when user ID is missing.")]
    public async Task CreateComplaint_WhenNoAuth_ShouldReturnUnauthorized()
    {
        var controller = CreateControllerWithoutAuth();
        var dto = new CreateComplaintDto { Content = "Valid content" };

        var result = await controller.CreateComplaint(dto);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    [AllureDescription("GetComplaints returns OK with paginated complaints list.")]
    public async Task GetComplaints_WithValidParams_ShouldReturnOk()
    {
        var userId = Guid.NewGuid();
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetCitizenComplaintsQuery>(), default))
            .ReturnsAsync(new object());

        var controller = CreateController(userId);

        var result = await controller.GetComplaints(page: 1, pageSize: 10);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    [AllureDescription("GetComplaints returns BadRequest when page is less than 1.")]
    public async Task GetComplaints_WithInvalidPagination_ShouldReturnBadRequest()
    {
        var controller = CreateController(Guid.NewGuid());

        var result = await controller.GetComplaints(page: 0, pageSize: 10);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("GetComplaints returns Unauthorized when user ID is missing.")]
    public async Task GetComplaints_WhenNoAuth_ShouldReturnUnauthorized()
    {
        var controller = CreateControllerWithoutAuth();

        var result = await controller.GetComplaints();

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    [AllureDescription("GetComplaintDetail returns OK for complaint owned by user.")]
    public async Task GetComplaintDetail_WhenOwned_ShouldReturnOk()
    {
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
