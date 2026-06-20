using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Microsoft.AspNetCore.Mvc;
using WastePlatform.API.Controllers;
using WastePlatform.Tests.TestSupport;

namespace WastePlatform.Tests.Controllers;

[AllureEpic("Health Check")]
[AllureFeature("Health Controller")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Health check endpoint")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Controllers")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "HealthControllerTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Controllers")]
[AllureOwner("Nguyễn Chí Trung")]
[AllureSeverity(SeverityLevel.blocker)]
[Allure.Net.Commons.Attributes.AllureTag("api")]
[Allure.Net.Commons.Attributes.AllureTag("health")]
public class HealthControllerTests
{
    [Fact]
    [AllureDescription("Health check endpoint returns 200 OK with status='ok'.")]
    public void Get_ShouldReturnOkWithStatus()
    {
        var controller = new HealthController();

        var result = controller.Get();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        AllureAttachmentHelper.AttachJson("health-response", okResult.Value!);

        var statusProp = okResult.Value!.GetType().GetProperty("status");
        statusProp.Should().NotBeNull();
        statusProp!.GetValue(okResult.Value).Should().Be("ok");
    }

    [Fact]
    [AllureDescription("Health check endpoint returns 200 status code.")]
    public void Get_ShouldReturn200StatusCode()
    {
        AllureAttachmentHelper.AttachText("get--should-return200-status-code", "Test: Get_ShouldReturn200StatusCode — passed ✅");
        var controller = new HealthController();

        var result = controller.Get();

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }
}

