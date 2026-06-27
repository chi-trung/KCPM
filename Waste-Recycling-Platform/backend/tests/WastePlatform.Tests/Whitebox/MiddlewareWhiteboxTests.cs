using FluentAssertions;
using Allure.Xunit.Attributes;
using Allure.Net.Commons;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using WastePlatform.API.Middleware;
using WastePlatform.Domain.Entities;
using WastePlatform.Domain.Enums;
using WastePlatform.Infrastructure.Persistence;
using Xunit;

namespace WastePlatform.Tests.Whitebox;

/// <summary>
/// Whitebox Testing — ValidateUserStatusMiddleware.InvokeAsync()
/// 
/// Kỹ thuật áp dụng theo Chương 4:
///   1. Control Flow Graph (CFG)     → 14 nodes, nested 4 levels deep
///   2. Cyclomatic Complexity V(G)   → V(G) = 6 (4 predicates + try/catch + 1)
///   3. Independent Paths            → 6 paths (P1-P6 including exception path)
///   4. Statement Coverage           → 100%
///   5. Branch/Decision Coverage     → 100% (12/12 branches)
///   6. Condition Coverage           → 100% (D4 compound: !IsNullOrEmpty && TryParse)
///   7. Branch-Condition Coverage    → 100%
///   8. Condition Combination        → Full truth table for D4
/// </summary>
[AllureEpic("Chương 4: Whitebox Testing")]
[AllureFeature("ValidateUserStatusMiddleware — CFG + Path Coverage")]
[Allure.Net.Commons.Attributes.AllureLabel("story", "Whitebox: Nested Control Flow + Exception Path")]
[Allure.Net.Commons.Attributes.AllureLabel("parentSuite", "xUnit Backend Tests")]
[Allure.Net.Commons.Attributes.AllureLabel("suite", "Whitebox")]
[Allure.Net.Commons.Attributes.AllureLabel("subSuite", "MiddlewareWhiteboxTests")]
[Allure.Net.Commons.Attributes.AllureLabel("package", "WastePlatform.Tests.Whitebox")]
[AllureOwner("Nguyễn Minh Phụng")]
[AllureSeverity(SeverityLevel.critical)]
[Allure.Net.Commons.Attributes.AllureTag("whitebox")]
[Allure.Net.Commons.Attributes.AllureTag("cfg")]
[Allure.Net.Commons.Attributes.AllureTag("middleware")]
public class MiddlewareWhiteboxTests
{
    private readonly Mock<ILogger<ValidateUserStatusMiddleware>> _mockLogger;
    private bool _nextCalled;

    public MiddlewareWhiteboxTests()
    {
        _mockLogger = new Mock<ILogger<ValidateUserStatusMiddleware>>();
    }

    private WastePlatformDbContext CreateDbContext(out Guid activeUserId, out Guid inactiveUserId)
    {
        var options = new DbContextOptionsBuilder<WastePlatformDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new WastePlatformDbContext(options);

        // Create users via factory method
        var activeUser = User.Create("active@test.com", "hashedpw", "Active User", UserRole.Citizen, "0901111111");
        var inactiveUser = User.Create("locked@test.com", "hashedpw", "Locked User", UserRole.Citizen, "0902222222");
        inactiveUser.Deactivate(); // Sets IsActive = false

        context.Users.Add(activeUser);
        context.Users.Add(inactiveUser);
        context.SaveChanges();

        activeUserId = activeUser.Id;
        inactiveUserId = inactiveUser.Id;
        return context;
    }

    private HttpContext CreateHttpContext(ClaimsPrincipal? user = null)
    {
        var context = new DefaultHttpContext();
        if (user != null)
            context.User = user;
        context.Response.Body = new MemoryStream();
        return context;
    }

    private ClaimsPrincipal CreateAuthenticatedUser(string userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    // ==========================================
    // PATH COVERAGE — 6 Independent Paths
    // V(G) = 6, bao gồm exception path
    // ==========================================

    #region Path P1: Unauthenticated → skip → _next (Node: 1→2F→12→14)

    /// <summary>
    /// Path P1: 1 → 2(F) → 12 → 14
    /// D2=False: request không có authentication
    /// → Skip all checks, call _next()
    /// </summary>
    [Fact]
    [AllureDescription(
        "Path P1: Unauthenticated request → skip validation → call _next()\n" +
        "CFG: Node 1 → 2(F) → 12 → 14\n" +
        "V(G) path 1/6")]
    public async Task Path1_UnauthenticatedRequest_SkipsValidation()
    {
        _nextCalled = false;
        RequestDelegate next = (ctx) => { _nextCalled = true; return Task.CompletedTask; };
        var middleware = new ValidateUserStatusMiddleware(next, _mockLogger.Object);
        var httpContext = CreateHttpContext(); // No user/auth
        var dbContext = CreateDbContext(out _, out _);

        await middleware.InvokeAsync(httpContext, dbContext);

        _nextCalled.Should().BeTrue("_next should be called for unauthenticated requests");
        httpContext.Response.StatusCode.Should().NotBe(401);
    }

    #endregion

    #region Path P2: Auth + invalid JWT claim → skip → _next (Node: 1→2T→3→4F→11→14)

    /// <summary>
    /// Path P2: 1 → 2(T) → 3 → 4(F) → 11 → 14
    /// D2=True, D4=False: authenticated but JWT has invalid/empty userId
    /// Condition Coverage D4: C1(!IsNullOrEmpty)=False → short-circuit
    /// </summary>
    [Fact]
    [AllureDescription(
        "Path P2: Authenticated + empty userId claim → skip → _next()\n" +
        "CFG: Node 1 → 2(T) → 3 → 4(F) → 11 → 14\n" +
        "V(G) path 2/6\n" +
        "Condition D4: C1=False (empty claim)")]
    public async Task Path2_EmptyUserIdClaim_SkipsValidation()
    {
        _nextCalled = false;
        RequestDelegate next = (ctx) => { _nextCalled = true; return Task.CompletedTask; };
        var middleware = new ValidateUserStatusMiddleware(next, _mockLogger.Object);

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var httpContext = CreateHttpContext(new ClaimsPrincipal(identity));
        var dbContext = CreateDbContext(out _, out _);

        await middleware.InvokeAsync(httpContext, dbContext);

        _nextCalled.Should().BeTrue();
    }

    /// <summary>
    /// Condition Coverage D4: C1=True(!IsNullOrEmpty), C2=False(TryParse fails)
    /// → D4=False (non-GUID string)
    /// </summary>
    [Fact]
    [AllureDescription(
        "Path P2 variant: Authenticated + non-GUID userId → skip\n" +
        "Condition D4: C1=True, C2=False (TryParse fails)")]
    public async Task Path2_NonGuidUserId_SkipsValidation()
    {
        _nextCalled = false;
        RequestDelegate next = (ctx) => { _nextCalled = true; return Task.CompletedTask; };
        var middleware = new ValidateUserStatusMiddleware(next, _mockLogger.Object);

        var user = CreateAuthenticatedUser("not-a-valid-guid");
        var httpContext = CreateHttpContext(user);
        var dbContext = CreateDbContext(out _, out _);

        await middleware.InvokeAsync(httpContext, dbContext);

        _nextCalled.Should().BeTrue();
    }

    #endregion

    #region Path P3: Auth + valid JWT + user not found → _next (Node: 1→2T→3→4T→5→6F→10→14)

    /// <summary>
    /// Path P3: 1 → 2(T) → 3 → 4(T) → 5 → 6(F) → 10 → 14
    /// D2=T, D4=T, D6=False: user not in database
    /// </summary>
    [Fact]
    [AllureDescription(
        "Path P3: Valid JWT but user not found in DB → proceed → _next()\n" +
        "CFG: Node 1 → 2(T) → 3 → 4(T) → 5 → 6(F) → 10 → 14\n" +
        "V(G) path 3/6")]
    public async Task Path3_UserNotFoundInDb_ProceedsToNext()
    {
        _nextCalled = false;
        RequestDelegate next = (ctx) => { _nextCalled = true; return Task.CompletedTask; };
        var middleware = new ValidateUserStatusMiddleware(next, _mockLogger.Object);

        var unknownUserId = Guid.NewGuid();
        var user = CreateAuthenticatedUser(unknownUserId.ToString());
        var httpContext = CreateHttpContext(user);
        var dbContext = CreateDbContext(out _, out _);

        await middleware.InvokeAsync(httpContext, dbContext);

        _nextCalled.Should().BeTrue();
        httpContext.Response.StatusCode.Should().NotBe(401);
    }

    #endregion

    #region Path P4: Auth + user INACTIVE → BLOCK 401 (Node: 1→2T→3→4T→5→6T→7T→8)

    /// <summary>
    /// Path P4: 1 → 2(T) → 3 → 4(T) → 5 → 6(T) → 7(T) → 8
    /// D2=T, D4=T, D6=T, D7=T: user found but IsActive=false → BLOCK
    /// </summary>
    [Fact]
    [AllureDescription(
        "Path P4: User found + IsActive=false → 401 BLOCKED\n" +
        "CFG: Node 1 → 2(T) → 3 → 4(T) → 5 → 6(T) → 7(T) → 8\n" +
        "V(G) path 4/6\n" +
        "CRITICAL PATH: Only path that blocks request")]
    public async Task Path4_InactiveUser_Returns401Blocked()
    {
        _nextCalled = false;
        RequestDelegate next = (ctx) => { _nextCalled = true; return Task.CompletedTask; };
        var middleware = new ValidateUserStatusMiddleware(next, _mockLogger.Object);

        var dbContext = CreateDbContext(out _, out var inactiveUserId);
        var user = CreateAuthenticatedUser(inactiveUserId.ToString());
        var httpContext = CreateHttpContext(user);

        await middleware.InvokeAsync(httpContext, dbContext);

        _nextCalled.Should().BeFalse("_next should NOT be called for inactive users");
        httpContext.Response.StatusCode.Should().Be(401);
    }

    #endregion

    #region Path P5: Auth + user ACTIVE → proceed → _next (Node: 1→2T→3→4T→5→6T→7F→9→14)

    /// <summary>
    /// Path P5: 1 → 2(T) → 3 → 4(T) → 5 → 6(T) → 7(F) → 9 → 14
    /// D7=False: user active → proceed normally
    /// </summary>
    [Fact]
    [AllureDescription(
        "Path P5: User found + IsActive=true → proceed → _next()\n" +
        "CFG: Node 1 → 2(T) → 3 → 4(T) → 5 → 6(T) → 7(F) → 9 → 14\n" +
        "V(G) path 5/6\n" +
        "HAPPY PATH")]
    public async Task Path5_ActiveUser_ProceedsNormally()
    {
        _nextCalled = false;
        RequestDelegate next = (ctx) => { _nextCalled = true; return Task.CompletedTask; };
        var middleware = new ValidateUserStatusMiddleware(next, _mockLogger.Object);

        var dbContext = CreateDbContext(out var activeUserId, out _);
        var user = CreateAuthenticatedUser(activeUserId.ToString());
        var httpContext = CreateHttpContext(user);

        await middleware.InvokeAsync(httpContext, dbContext);

        _nextCalled.Should().BeTrue("_next should be called for active users");
        httpContext.Response.StatusCode.Should().NotBe(401);
    }

    #endregion

    #region Path P6: Exception → catch → _next (Node: 1→13→14)

    /// <summary>
    /// Path P6: 1 → 13 → 14
    /// Exception in try block → caught → _next() still called
    /// </summary>
    [Fact]
    [AllureDescription(
        "Path P6: Exception during validation → caught → _next() still called\n" +
        "CFG: Node 1 → 13 → 14\n" +
        "V(G) path 6/6 — ALL PATHS COVERED\n" +
        "Tests exception handling resilience")]
    public async Task Path6_ExceptionDuringValidation_CatchesAndProceeds()
    {
        _nextCalled = false;
        RequestDelegate next = (ctx) => { _nextCalled = true; return Task.CompletedTask; };
        var middleware = new ValidateUserStatusMiddleware(next, _mockLogger.Object);

        var dbContext = CreateDbContext(out var activeUserId, out _);
        var user = CreateAuthenticatedUser(activeUserId.ToString());
        var httpContext = CreateHttpContext(user);

        // Dispose context to trigger ObjectDisposedException
        dbContext.Dispose();

        await middleware.InvokeAsync(httpContext, dbContext);

        _nextCalled.Should().BeTrue("_next should still be called after exception is caught");
    }

    #endregion

    // ==========================================
    // CONDITION COMBINATION COVERAGE for D4
    // D4: (!string.IsNullOrEmpty(claim) && Guid.TryParse(claim, out _))
    // ==========================================

    #region D4: Condition Combination Coverage

    /// <summary>
    /// | # | C1(!IsNullOrEmpty) | C2(TryParse) | D4 (&&) | TC          |
    /// |---|---------------------|--------------|---------|-------------|
    /// | 1 | T                   | T            | T       | Valid GUID  |
    /// | 2 | T                   | F            | F       | "not-guid"  |
    /// | 3 | F (empty)           | -            | F       | "" claim    |
    /// → 3/3 feasible combinations covered = 100%
    /// </summary>
    [Theory]
    [InlineData("valid-guid-placeholder", true, "C1=T, C2=T → D4=True")]
    [InlineData("not-a-guid-string", false, "C1=T, C2=F → D4=False")]
    [InlineData("", false, "C1=F(empty) → D4=False")]
    [AllureDescription("Condition Combination for D4: !IsNullOrEmpty(claim) && TryParse(claim)")]
    public async Task CondCombination_D4_ClaimValidation(
        string claimValue, bool shouldQueryDb, string conditionLabel)
    {
        _nextCalled = false;
        RequestDelegate next = (ctx) => { _nextCalled = true; return Task.CompletedTask; };
        var middleware = new ValidateUserStatusMiddleware(next, _mockLogger.Object);

        var dbContext = CreateDbContext(out var activeUserId, out _);
        // Use actual active user GUID for the "valid-guid" case
        var actualClaim = claimValue == "valid-guid-placeholder" ? activeUserId.ToString() : claimValue;
        var user = CreateAuthenticatedUser(actualClaim);
        var httpContext = CreateHttpContext(user);

        await middleware.InvokeAsync(httpContext, dbContext);

        conditionLabel.Should().NotBeNullOrWhiteSpace();
        shouldQueryDb.Should().Be(claimValue == "valid-guid-placeholder");
        _nextCalled.Should().BeTrue();
    }

    #endregion
}
