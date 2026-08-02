using System.Security.Claims;
using System.Text.Json;
using HeimReport.Api.DTOs.AuditLogs;
using HeimReport.Api.Entities;
using HeimReport.Api.Repositories.AuditLogs;
using HeimReport.Api.Services.AuditLogs;
using Microsoft.AspNetCore.Http;
using MockQueryable;
using Moq;

namespace HeimReport.Api.UnitTests.Services.AuditLogs;

public class AuditLogServiceTests
{
    private readonly Mock<IAuditLogRepository> _auditLogRepository = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor = new();

    private readonly AuditLogService _sut;

    public AuditLogServiceTests()
    {
        _sut = new AuditLogService(_auditLogRepository.Object, _httpContextAccessor.Object);
    }

    // ===================== HELPERS =====================

    private void SetAuthenticatedUser(int userId, string? ipAddress = "127.0.0.1")
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };

        if (ipAddress is not null)
        {
            httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(ipAddress);
        }

        _httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);
    }

    private void SetAnonymousUser()
    {
        var httpContext = new DefaultHttpContext();
        _httpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);
    }

    private void SetNoHttpContext()
    {
        _httpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);
    }

    // ===================== LOGASYNC — USER RESOLUTION =====================

    [Fact]
    public async Task LogAsync_ShouldCaptureUserId_WhenUserIsAuthenticated()
    {
        // Arrange
        SetAuthenticatedUser(userId: 42);

        AuditLog? captured = null;
        _auditLogRepository
            .Setup(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((log, _) => captured = log)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.LogAsync("CREATE_EMPLOYEE", nameof(Employee), 1);

        // Assert
        Assert.NotNull(captured);
        Assert.Equal(42, captured!.UserId);
    }

    [Fact]
    public async Task LogAsync_ShouldSetUserIdToNull_WhenUserIsNotAuthenticated()
    {
        // Arrange
        SetAnonymousUser();

        AuditLog? captured = null;
        _auditLogRepository
            .Setup(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((log, _) => captured = log)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.LogAsync("SYSTEM_ACTION");

        // Assert
        Assert.NotNull(captured);
        Assert.Null(captured!.UserId);
    }

    [Fact]
    public async Task LogAsync_ShouldSetUserIdToNull_WhenHttpContextIsUnavailable()
    {
        // Arrange
        SetNoHttpContext();

        AuditLog? captured = null;
        _auditLogRepository
            .Setup(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((log, _) => captured = log)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.LogAsync("BACKGROUND_JOB_ACTION");

        // Assert
        Assert.NotNull(captured);
        Assert.Null(captured!.UserId);
    }

    // ===================== LOGASYNC — IP ADDRESS =====================

    [Fact]
    public async Task LogAsync_ShouldCaptureIpAddress_WhenAvailable()
    {
        // Arrange
        SetAuthenticatedUser(userId: 1, ipAddress: "203.0.113.5");

        AuditLog? captured = null;
        _auditLogRepository
            .Setup(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((log, _) => captured = log)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.LogAsync("LOGIN");

        // Assert
        Assert.NotNull(captured);
        Assert.Equal("203.0.113.5", captured!.IpAddress);
    }

    // ===================== LOGASYNC — SERIALIZATION =====================

    [Fact]
    public async Task LogAsync_ShouldSerializeOldAndNewValues_WhenProvided()
    {
        // Arrange
        SetAuthenticatedUser(userId: 1);
        var oldVals = new { Name = "Old Name" };
        var newVals = new { Name = "New Name" };

        AuditLog? captured = null;
        _auditLogRepository
            .Setup(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((log, _) => captured = log)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.LogAsync("UPDATE_COUNTRY", nameof(Country), 5, oldValues: oldVals, newValues: newVals);

        // Assert
        Assert.NotNull(captured);
        Assert.NotNull(captured!.OldValues);
        Assert.NotNull(captured.NewValues);

        var deserializedOld = JsonSerializer.Deserialize<Dictionary<string, string>>(captured.OldValues!);
        var deserializedNew = JsonSerializer.Deserialize<Dictionary<string, string>>(captured.NewValues!);

        Assert.Equal("Old Name", deserializedOld!["Name"]);
        Assert.Equal("New Name", deserializedNew!["Name"]);
    }

    [Fact]
    public async Task LogAsync_ShouldLeaveOldAndNewValuesNull_WhenNotProvided()
    {
        // Arrange
        SetAuthenticatedUser(userId: 1);

        AuditLog? captured = null;
        _auditLogRepository
            .Setup(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((log, _) => captured = log)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.LogAsync("LOGOUT", nameof(User), 1);

        // Assert
        Assert.NotNull(captured);
        Assert.Null(captured!.OldValues);
        Assert.Null(captured.NewValues);
    }

    [Fact]
    public async Task LogAsync_ShouldNeverSerializeSensitiveFields_WhenCallerOmitsThem()
    {
        // Arrange
        SetAuthenticatedUser(userId: 1);
        var newVals = new { Username = "jane.doe", Role = "HR" };

        AuditLog? captured = null;
        _auditLogRepository
            .Setup(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((log, _) => captured = log)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.LogAsync("PROVISION_USER", nameof(User), 1, oldValues: null, newValues: newVals);

        // Assert
        Assert.NotNull(captured);
        Assert.DoesNotContain("PasswordHash", captured!.NewValues);
        Assert.DoesNotContain("TokenHash", captured.NewValues);
    }

    // ===================== LOGASYNC — BASIC FIELDS + PERSISTENCE =====================

    [Fact]
    public async Task LogAsync_ShouldSetActionEntityNameEntityIdAndTimestamp()
    {
        // Arrange
        SetAuthenticatedUser(userId: 1);

        AuditLog? captured = null;
        _auditLogRepository
            .Setup(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((log, _) => captured = log)
            .Returns(Task.CompletedTask);

        var before = DateTime.UtcNow;

        // Act
        await _sut.LogAsync("DELETE_POSITION", nameof(Position), 7);

        // Assert
        var after = DateTime.UtcNow;

        Assert.NotNull(captured);
        Assert.Equal("DELETE_POSITION", captured!.Action);
        Assert.Equal(nameof(Position), captured.EntityName);
        Assert.Equal(7, captured.EntityId);
        Assert.InRange(captured.Timestamp, before, after);
    }

    [Fact]
    public async Task LogAsync_ShouldPersistViaSaveChangesAsync()
    {
        // Arrange
        SetAuthenticatedUser(userId: 1);

        // Act
        await _sut.LogAsync("CREATE_COUNTRY");

        // Assert
        _auditLogRepository.Verify(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Once);
        _auditLogRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ===================== GETPAGEDASYNC — FILTERS =====================

    [Fact]
    public async Task GetPagedAsync_ShouldFilterByUserId()
    {
        // Arrange
        var logs = new List<AuditLog>
        {
            new() { Id = 1, UserId = 1, Action = "A", Timestamp = DateTime.UtcNow },
            new() { Id = 2, UserId = 2, Action = "B", Timestamp = DateTime.UtcNow }
        };

        _auditLogRepository.Setup(r => r.QueryWithDetails()).Returns(logs.BuildMock());

        var query = new AuditLogQueryDto { PageNumber = 1, PageSize = 10, UserId = 1 };

        // Act
        var result = await _sut.GetPagedAsync(query);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldFilterByAction()
    {
        // Arrange
        var logs = new List<AuditLog>
        {
            new() { Id = 1, Action = "CREATE_EMPLOYEE", Timestamp = DateTime.UtcNow },
            new() { Id = 2, Action = "DELETE_EMPLOYEE", Timestamp = DateTime.UtcNow }
        };

        _auditLogRepository.Setup(r => r.QueryWithDetails()).Returns(logs.BuildMock());

        var query = new AuditLogQueryDto { PageNumber = 1, PageSize = 10, Action = "CREATE_EMPLOYEE" };

        // Act
        var result = await _sut.GetPagedAsync(query);

        // Assert
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldFilterByEntityNameAndEntityId()
    {
        // Arrange
        var logs = new List<AuditLog>
        {
            new() { Id = 1, Action = "UPDATE_EMPLOYEE", EntityName = nameof(Employee), EntityId = 10, Timestamp = DateTime.UtcNow },
            new() { Id = 2, Action = "UPDATE_EMPLOYEE", EntityName = nameof(Employee), EntityId = 20, Timestamp = DateTime.UtcNow },
            new() { Id = 3, Action = "UPDATE_COUNTRY", EntityName = nameof(Country), EntityId = 10, Timestamp = DateTime.UtcNow }
        };

        _auditLogRepository.Setup(r => r.QueryWithDetails()).Returns(logs.BuildMock());

        var query = new AuditLogQueryDto
        {
            PageNumber = 1,
            PageSize = 10,
            EntityName = nameof(Employee),
            EntityId = 10
        };

        // Act
        var result = await _sut.GetPagedAsync(query);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(1, result.Items[0].Id);
        Assert.Equal(10, result.Items[0].EntityId);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldFilterByDateRange()
    {
        // Arrange
        var logs = new List<AuditLog>
        {
            new() { Id = 1, Action = "A", Timestamp = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new() { Id = 2, Action = "B", Timestamp = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc) },
            new() { Id = 3, Action = "C", Timestamp = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc) }
        };

        _auditLogRepository.Setup(r => r.QueryWithDetails()).Returns(logs.BuildMock());

        var query = new AuditLogQueryDto
        {
            PageNumber = 1,
            PageSize = 10,
            FromDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            ToDate = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var result = await _sut.GetPagedAsync(query);

        // Assert
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnAllOrderedByTimestampDescending_WhenNoFiltersApplied()
    {
        // Arrange
        var older = new AuditLog { Id = 1, Action = "A", Timestamp = DateTime.UtcNow.AddDays(-2) };
        var newer = new AuditLog { Id = 2, Action = "B", Timestamp = DateTime.UtcNow };

        var logs = new List<AuditLog> { older, newer };

        _auditLogRepository.Setup(r => r.QueryWithDetails()).Returns(logs.BuildMock());

        var query = new AuditLogQueryDto { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.GetPagedAsync(query);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(newer.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldRespectPagination()
    {
        // Arrange
        var logs = Enumerable.Range(1, 25)
            .Select(i => new AuditLog { Id = i, Action = "A", Timestamp = DateTime.UtcNow.AddMinutes(-i) })
            .ToList();

        _auditLogRepository.Setup(r => r.QueryWithDetails()).Returns(logs.BuildMock());

        var query = new AuditLogQueryDto { PageNumber = 2, PageSize = 10 };

        // Act
        var result = await _sut.GetPagedAsync(query);

        // Assert
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(10, result.Items.Count);
        Assert.Equal(2, result.PageNumber);
    }
}