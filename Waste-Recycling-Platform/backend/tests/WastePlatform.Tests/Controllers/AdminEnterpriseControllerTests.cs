using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WastePlatform.API.Controllers;
using WastePlatform.Application.Admin.Enterprises.Commands;
using WastePlatform.Application.Admin.Enterprises.DTOs;
using WastePlatform.Application.Admin.Enterprises.Queries;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Controllers;

[AllureEpic("Admin APIs")]
[AllureFeature("Admin Enterprise Controller")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Enterprise management: list, detail, verify, reject")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "AdminEnterpriseControllerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("Chi Trung")]
[AllureSeverity(SeverityLevel.normal)]
[Allure.Net.Commons.Attributes.AllureTag("api")]
[Allure.Net.Commons.Attributes.AllureTag("admin")]
[Allure.Net.Commons.Attributes.AllureTag("enterprise")]
public class AdminEnterpriseControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();

    #region GetEnterprises

    [Fact]
    [AllureDescription("GetEnterprises returns OK with paginated list.")]
    public async Task GetEnterprises_ShouldReturnOk()
    {
        var enterprises = new List<EnterpriseListDto>
        {
            new() { Id = Guid.NewGuid(), CompanyName = "Green Corp" }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetEnterprisesQuery>(), default))
            .ReturnsAsync((enterprises.AsEnumerable(), 1, 1));

        var controller = new AdminEnterpriseController(_mediatorMock.Object);

        var result = await controller.GetEnterprises();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        AllureAttachmentHelper.AttachJson("enterprises-response", okResult.Value!);
    }

    [Fact]
    [AllureDescription("GetEnterprises passes filter params to query.")]
    public async Task GetEnterprises_WithFilters_ShouldPassParams()
    {
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetEnterprisesQuery>(q =>
                q.Page == 2 && q.PageSize == 5 && q.IsVerified == true && q.SearchTerm == "green"), default))
            .ReturnsAsync((Enumerable.Empty<EnterpriseListDto>(), 0, 0));

        var controller = new AdminEnterpriseController(_mediatorMock.Object);

        await controller.GetEnterprises(page: 2, pageSize: 5, isVerified: true, searchTerm: "green");

        _mediatorMock.Verify(m => m.Send(It.Is<GetEnterprisesQuery>(q =>
            q.Page == 2 && q.PageSize == 5 && q.IsVerified == true), default), Times.Once);
    }

    [Fact]
    [AllureDescription("GetEnterprises returns 500 on exception.")]
    public async Task GetEnterprises_WhenException_ShouldReturn500()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetEnterprisesQuery>(), default))
            .ThrowsAsync(new Exception("DB error"));

        var controller = new AdminEnterpriseController(_mediatorMock.Object);

        var result = await controller.GetEnterprises();

        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetEnterpriseDetail

    [Fact]
    [AllureDescription("GetEnterpriseDetail returns OK when enterprise exists.")]
    public async Task GetEnterpriseDetail_WhenExists_ShouldReturnOk()
    {
        var enterpriseId = Guid.NewGuid();
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetEnterpriseDetailQuery>(q => q.EnterpriseId == enterpriseId), default))
            .ReturnsAsync(new EnterpriseDetailDto
            {
                Id = enterpriseId,
                CompanyName = "Green Corp",
                IsVerified = true
            });

        var controller = new AdminEnterpriseController(_mediatorMock.Object);

        var result = await controller.GetEnterpriseDetail(enterpriseId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    [AllureDescription("GetEnterpriseDetail returns NotFound when enterprise doesn't exist.")]
    public async Task GetEnterpriseDetail_WhenNotFound_ShouldReturnNotFound()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetEnterpriseDetailQuery>(), default))
            .ReturnsAsync((EnterpriseDetailDto?)null);

        var controller = new AdminEnterpriseController(_mediatorMock.Object);

        var result = await controller.GetEnterpriseDetail(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region VerifyEnterprise

    [Fact]
    [AllureDescription("VerifyEnterprise returns OK when successful.")]
    public async Task VerifyEnterprise_WhenSuccess_ShouldReturnOk()
    {
        var enterpriseId = Guid.NewGuid();
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<VerifyEnterpriseCommand>(), default))
            .ReturnsAsync(new VerifyEnterpriseResult
            {
                Success = true,
                Message = "Enterprise verified",
                EnterpriseId = enterpriseId
            });

        var controller = new AdminEnterpriseController(_mediatorMock.Object);

        var result = await controller.VerifyEnterprise(enterpriseId);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    [AllureDescription("VerifyEnterprise returns NotFound when enterprise doesn't exist.")]
    public async Task VerifyEnterprise_WhenNotFound_ShouldReturnNotFound()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<VerifyEnterpriseCommand>(), default))
            .ReturnsAsync(new VerifyEnterpriseResult { Success = false, Message = "Not found" });

        var controller = new AdminEnterpriseController(_mediatorMock.Object);

        var result = await controller.VerifyEnterprise(Guid.NewGuid());

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region RejectEnterprise

    [Fact]
    [AllureDescription("RejectEnterprise returns BadRequest when reason is empty.")]
    public async Task RejectEnterprise_EmptyReason_ShouldReturnBadRequest()
    {
        var controller = new AdminEnterpriseController(_mediatorMock.Object);
        var request = new RejectEnterpriseRequest { ReasonForRejection = "  " };

        var result = await controller.RejectEnterprise(Guid.NewGuid(), request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    [AllureDescription("RejectEnterprise returns OK when successful.")]
    public async Task RejectEnterprise_WithValidData_ShouldReturnOk()
    {
        var enterpriseId = Guid.NewGuid();
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RejectEnterpriseCommand>(), default))
            .ReturnsAsync(new RejectEnterpriseResult
            {
                Success = true,
                Message = "Enterprise rejected",
                EnterpriseId = enterpriseId
            });

        var controller = new AdminEnterpriseController(_mediatorMock.Object);
        var request = new RejectEnterpriseRequest { ReasonForRejection = "Insufficient documentation" };

        var result = await controller.RejectEnterprise(enterpriseId, request);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    [AllureDescription("RejectEnterprise returns NotFound when enterprise doesn't exist.")]
    public async Task RejectEnterprise_WhenNotFound_ShouldReturnNotFound()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RejectEnterpriseCommand>(), default))
            .ReturnsAsync(new RejectEnterpriseResult { Success = false, Message = "Not found" });

        var controller = new AdminEnterpriseController(_mediatorMock.Object);
        var request = new RejectEnterpriseRequest { ReasonForRejection = "Invalid" };

        var result = await controller.RejectEnterprise(Guid.NewGuid(), request);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}
