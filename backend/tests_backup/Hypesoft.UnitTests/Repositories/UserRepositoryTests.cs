using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hypesoft.Domain.Entities;
using Hypesoft.Domain.Repositories;
using Hypesoft.Infrastructure.Persistence;
using Hypesoft.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Hypesoft.UnitTests.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly Mock<IAsyncCursor<ApplicationUser>> _mockUserCursor;
    private readonly Mock<IAsyncCursor<ApplicationRole>> _mockRoleCursor;
    private readonly Mock<IAsyncCursor<ApplicationUserRole>> _mockUserRoleCursor;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly UserRepository _userRepository;
    private readonly Mock<ILogger<UserRepository>> _mockLogger;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<RoleManager<ApplicationRole>> _mockRoleManager;
    private readonly Mock<IUserStore<ApplicationUser>> _mockUserStore;
    private readonly Mock<IRoleStore<ApplicationRole>> _mockRoleStore;
    private readonly List<ApplicationUser> _users;
    private readonly List<ApplicationRole> _roles;
    private readonly List<ApplicationUserRole> _userRoles;

    public UserRepositoryTests()
    {       
        _mockUserCursor = new Mock<IAsyncCursor<ApplicationUser>>();
        _mockRoleCursor = new Mock<IAsyncCursor<ApplicationRole>>();
        _mockUserRoleCursor = new Mock<IAsyncCursor<ApplicationUserRole>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<UserRepository>>();
        _mockUserStore = new Mock<IUserStore<ApplicationUser>>();
        _mockRoleStore = new Mock<IRoleStore<ApplicationRole>>();

        _users = new List<ApplicationUser>();
        _roles = new List<ApplicationRole>();
        _userRoles = new List<ApplicationUserRole>();

        // Setup mock UserManager and RoleManager
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            _mockUserStore.Object,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );

        _mockRoleManager = new Mock<RoleManager<ApplicationRole>>(
            _mockRoleStore.Object,
            null,
            null,
            null,
            null
        );

        // Setup mock ApplicationDbContext
        var mockContext = new Mock<ApplicationDbContext>();
        mockContext.Setup(c => c.Users).Returns(_mockUsersCollection.Object);
        mockContext.Setup(c => c.Roles).Returns(_mockRolesCollection.Object);
        mockContext.Setup(c => c.UserRoles).Returns(_mockUserRolesCollection.Object);

        // Setup mock FindAsync for users
        _mockUsersCollection
            .Setup(x =>
                x.FindAsync(
                    It.IsAny<FilterDefinition<ApplicationUser>>(),
                    It.IsAny<FindOptions<ApplicationUser, ApplicationUser>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(_mockUserCursor.Object);

        // Setup mock cursor for users
        _mockUserCursor
            .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false);
        _mockUserCursor.SetupGet(_ => _.Current).Returns(() => _users);

        // Create the repository instance
        _userRepository = new UserRepository(
            _mockUserManager.Object,
            _mockRoleManager.Object,
            mockContext.Object,
            _mockUnitOfWork.Object
        );
    }

    public void Dispose()
    {
        // Cleanup code if needed
    }

    [Fact]
    public async Task GetByIdAsync_UserExists_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "testuser",
            Email = "test@example.com",
        };
        _users.Add(user);

        // Act
        var result = await _userRepository.GetByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_UserExists_ReturnsUser()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = "test@example.com",
        };
        _users.Add(user);

        // Setup mock for FindAsync with email filter
        _mockUsersCollection
            .Setup(x =>
                x.FindAsync(
                    It.IsAny<FilterDefinition<ApplicationUser>>(),
                    It.IsAny<FindOptions<ApplicationUser, ApplicationUser>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                (
                    FilterDefinition<ApplicationUser> filter,
                    FindOptions<ApplicationUser, ApplicationUser> options,
                    CancellationToken token
                ) =>
                {
                    var email = filter.Render("email").ToString();
                    _mockUserCursor
                        .Setup(c => c.Current)
                        .Returns(_users.Where(u => u.Email == "test@example.com").ToList());
                    return _mockUserCursor.Object;
                }
            );

        // Act
        var result = await _userRepository.GetByEmailAsync("test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task AddAsync_ValidUser_AddsUser()
    {
        // Arrange
        var user = new ApplicationUser { UserName = "newuser", Email = "new@example.com" };
        _mockUserManager
            .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _userRepository.AddAsync(user);

        // Assert
        Assert.NotNull(result);
        _mockUserManager.Verify(
            x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateAsync_ValidUser_UpdatesUser()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "existinguser",
            Email = "existing@example.com",
        };
        _mockUserManager
            .Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _userRepository.UpdateAsync(user);

        // Assert
        _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ValidUserId_DeletesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "tobedeleted",
            Email = "delete@example.com",
        };
        _users.Add(user);

        _mockUserManager.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);
        _mockUserManager
            .Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await _userRepository.DeleteAsync(userId);

        // Assert
        _mockUserManager.Verify(x => x.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }

    [Fact]
    public async Task IsEmailUniqueAsync_EmailNotInUse_ReturnsTrue()
    {
        // Arrange
        _mockUserManager
            .Setup(x => x.FindByEmailAsync("unique@example.com"))
            .ReturnsAsync((ApplicationUser)null);

        // Act
        var result = await _userRepository.IsEmailUniqueAsync("unique@example.com");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task AddToRoleAsync_ValidUserAndRole_AddsUserToRole()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = "test@example.com",
        };
        var role = new ApplicationRole { Id = Guid.NewGuid(), Name = "Admin" };

        _mockRoleManager.Setup(x => x.FindByNameAsync("Admin")).ReturnsAsync(role);
        _mockUserManager
            .Setup(x => x.AddToRoleAsync(user, "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _userRepository.AddToRoleAsync(user, "Admin");

        // Assert
        Assert.True(result.Succeeded);
        _mockUserManager.Verify(x => x.AddToRoleAsync(user, "Admin"), Times.Once);
    }
}
