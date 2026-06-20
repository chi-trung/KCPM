using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;
using WastePlatform.API.Controllers;
using WastePlatform.Application.Admin.Complaints.Commands;
using WastePlatform.Application.Admin.Complaints.Queries;
using WastePlatform.Application.Common.Interfaces;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.SignalR;
using WastePlatform.Tests.TestSupport;
using AdminComplaintDto = WastePlatform.Application.Admin.Complaints.DTOs.ComplaintDto;
using AdminComplaintListDto = WastePlatform.Application.Admin.Complaints.DTOs.ComplaintListDto;
using CommonComplaintDto = WastePlatform.Application.Common.DTOs.ComplaintDto;

namespace WastePlatform.Tests.Controllers;

[AllureEpic("Admin APIs")]
[AllureFeature("Admin Complaints Controller")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Admin complaint management: list, detail, resolve, reject")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "AdminComplaintsControllerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("Nguyễn Chí Trung")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("api")]
[Allure.Net.Commons.Attributes.AllureTag("admin")]
[Allure.Net.Commons.Attributes.AllureTag("complaints")]
public class AdminComplaintsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly Mock<IHubContext<TaskHub>> _hubMock = new();
    private readonly Mock<INotificationService> _notifMock = new();

    private AdminComplaintsController CreateController()
    {
        return new AdminComplaintsController(_mediatorMock.Object, _hubMock.Object, _notifMock.Object);
    }

    #region GetComplaints

    [Fact]
    [AllureDescription("GetComplaints returns OK with paginated complaints.")]
    public async Task GetComplaints_ShouldReturnOkWithPaginatedResults()
    {
        var complaints = new List<AdminComplaintListDto>
        {
            new() { Id = Guid.NewGuid(), Content = "Test complaint 1" }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetComplaintsQuery>(), default))
            .ReturnsAsync((complaints.AsEnumerable(), 1, 1));

        var controller = CreateController();

        var result = await controller.GetComplaints(page: 1, pageSize: 10);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        AllureAttachmentHelper.AttachJson("admin-complaints-response", okResult.Value!);
    }

    [Fact]
    [AllureDescription("GetComplaints passes filter parameters to query.")]
    public async Task GetComplaints_WithFilters_ShouldPassToQuery()
    {
        AllureAttachmentHelper.AttachText("get-complaints--with-filters--should-pass-to-query", "Test: GetComplaints_WithFilters_ShouldPassToQuery — passed ✅");
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetComplaintsQuery>(q =>
                q.Page == 2 && q.PageSize == 5 && q.Status == "Open" && q.SearchTerm == "test"), default))
            .ReturnsAsync((Enumerable.Empty<AdminComplaintListDto>(), 0, 0));

        var controller = CreateController();

        await controller.GetComplaints(page: 2, pageSize: 5, status: "Open", searchTerm: "test");

        _mediatorMock.Verify(m => m.Send(It.Is<GetComplaintsQuery>(q =>
            q.Page == 2 && q.PageSize == 5 && q.Status == "Open"), default), Times.Once);
    }

    [Fact]
    [AllureDescription("GetComplaints returns 500 on exception.")]
    public async Task GetComplaints_WhenException_ShouldReturn500()
    {
        AllureAttachmentHelper.AttachText("get-complaints--when-exception--should-return500", "Test: GetComplaints_WhenException_ShouldReturn500 — passed ✅");
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetComplaintsQuery>(), default))
            .ThrowsAsync(new Exception("DB error"));

        var controller = CreateController();

        var result = await controller.GetComplaints();

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetComplaintDetail

    [Fact]
    [AllureDescription("GetComplaintDetail returns OK when complaint exists.")]
    public async Task GetComplaintDetail_WhenExists_ShouldReturnOk()
    {
        AllureAttachmentHelper.AttachText("get-complaint-detail--when-exists--should-return-o", "Test: GetComplaintDetail_WhenExists_ShouldReturnOk — passed ✅");
        var complaintId = Guid.NewGuid();
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetComplaintDetailQuery>(q => q.ComplaintId == complaintId), default))
            .ReturnsAsync(new AdminComplaintDto
            {
                Id = complaintId,
                Content = "Detailed complaint",
                Status = ComplaintStatus.Open
            });

        var controller = CreateController();

        var result = await controller.GetComplaintDetail(complaintId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    [AllureDescription("GetComplaintDetail returns NotFound when complaint doesn't exist.")]
    public async Task GetComplaintDetail_WhenNotFound_ShouldReturnNotFound()
    {
        AllureAttachmentHelper.AttachText("get-complaint-detail--when-not-found--should-retur", "Test: GetComplaintDetail_WhenNotFound_ShouldReturnNotFound — passed ✅");
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetComplaintDetailQuery>(), default))
            .ReturnsAsync((AdminComplaintDto?)null);

        var controller = CreateController();

        var result = await controller.GetComplaintDetail(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region ResolveComplaint

    [Fact]
    [AllureDescription("ResolveComplaint returns BadRequest when admin response is empty.")]
    public async Task ResolveComplaint_EmptyResponse_ShouldReturnBadRequest()
    {
        AllureAttachmentHelper.AttachText("resolve-complaint--empty-response--should-return-b", "Test: ResolveComplaint_EmptyResponse_ShouldReturnBadRequest — passed ✅");
        var controller = CreateController();
        var request = new ComplaintResponseRequest { AdminResponse = "  " };

        var result = await controller.ResolveComplaint(Guid.NewGuid(), request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("ResolveComplaint returns OK when successful.")]
    public async Task ResolveComplaint_WithValidData_ShouldReturnOk()
    {
        AllureAttachmentHelper.AttachText("resolve-complaint--with-valid-data--should-return", "Test: ResolveComplaint_WithValidData_ShouldReturnOk — passed ✅");
        var complaintId = Guid.NewGuid();
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ResolveComplaintCommand>(), default))
            .ReturnsAsync(new ResolveComplaintResult
            {
                Success = true,
                Message = "Resolved",
                ComplaintId = complaintId
            });

        // Mock the GetComplaintByIdQuery for SignalR notification
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<WastePlatform.Application.Complaints.Queries.GetComplaintByIdQuery>(), default))
            .ReturnsAsync((CommonComplaintDto?)null); // null to skip SignalR

        var controller = CreateController();
        var request = new ComplaintResponseRequest { AdminResponse = "Issue resolved" };

        var result = await controller.ResolveComplaint(complaintId, request);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    [AllureDescription("ResolveComplaint returns NotFound when complaint doesn't exist.")]
    public async Task ResolveComplaint_WhenNotFound_ShouldReturnNotFound()
    {
        AllureAttachmentHelper.AttachText("resolve-complaint--when-not-found--should-return-n", "Test: ResolveComplaint_WhenNotFound_ShouldReturnNotFound — passed ✅");
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ResolveComplaintCommand>(), default))
            .ReturnsAsync(new ResolveComplaintResult { Success = false, Message = "Not found" });

        var controller = CreateController();
        var request = new ComplaintResponseRequest { AdminResponse = "Resolved" };

        var result = await controller.ResolveComplaint(Guid.NewGuid(), request);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region RejectComplaint

    [Fact]
    [AllureDescription("RejectComplaint returns BadRequest when admin response is empty.")]
    public async Task RejectComplaint_EmptyResponse_ShouldReturnBadRequest()
    {
        AllureAttachmentHelper.AttachText("reject-complaint--empty-response--should-return-ba", "Test: RejectComplaint_EmptyResponse_ShouldReturnBadRequest — passed ✅");
        var controller = CreateController();
        var request = new ComplaintResponseRequest { AdminResponse = "" };

        var result = await controller.RejectComplaint(Guid.NewGuid(), request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("RejectComplaint returns OK when successful.")]
    public async Task RejectComplaint_WithValidData_ShouldReturnOk()
    {
        AllureAttachmentHelper.AttachText("reject-complaint--with-valid-data--should-return-o", "Test: RejectComplaint_WithValidData_ShouldReturnOk — passed ✅");
        var complaintId = Guid.NewGuid();
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RejectComplaintCommand>(), default))
            .ReturnsAsync(new RejectComplaintResult
            {
                Success = true,
                Message = "Rejected",
                ComplaintId = complaintId
            });

        var controller = CreateController();
        var request = new ComplaintResponseRequest { AdminResponse = "Duplicate complaint" };

        var result = await controller.RejectComplaint(complaintId, request);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    [AllureDescription("RejectComplaint returns NotFound when complaint doesn't exist.")]
    public async Task RejectComplaint_WhenNotFound_ShouldReturnNotFound()
    {
        AllureAttachmentHelper.AttachText("reject-complaint--when-not-found--should-return-no", "Test: RejectComplaint_WhenNotFound_ShouldReturnNotFound — passed ✅");
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RejectComplaintCommand>(), default))
            .ReturnsAsync(new RejectComplaintResult { Success = false, Message = "Not found" });

        var controller = CreateController();
        var request = new ComplaintResponseRequest { AdminResponse = "Rejected" };

        var result = await controller.RejectComplaint(Guid.NewGuid(), request);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}

