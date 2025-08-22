using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ardalis.Specification;
using Hypesoft.Domain.Entities;
using Hypesoft.Infrastructure.Data;
using Hypesoft.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Hypesoft.IntegrationTests.Repositories
{
    public class UserRepositoryTests : IClassFixture<TestBase>, IDisposable
    {
        private readonly UserRepository _userRepository;
        private readonly ApplicationDbContext _dbContext;
        private readonly TestBase _testBase;

        public UserRepositoryTests(TestBase testBase)
        {
            _testBase = testBase;
            _dbContext = _testBase.DbContext;

            // Get logger from service provider
            var loggerFactory = _testBase.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<UserRepository>();

            _userRepository = new UserRepository(_dbContext, logger);

            // Ensure we start with a clean database
            ClearDatabase().Wait();
        }

        public void Dispose()
        {
            // Clean up after each test
            ClearDatabase().Wait();
        }

        private async Task ClearDatabase()
        {
            // ✅ Use EF Core em vez de MongoDB Driver
            _dbContext.Users.RemoveRange(_dbContext.Users);
            _dbContext.Roles.RemoveRange(_dbContext.Roles);
            await _dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task AddAsync_ShouldAddUserToDatabase()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
            };

            // Act
            await _userRepository.AddAsync(user);
            await _dbContext.SaveChangesAsync(); // ✅ Salvar via EF Core

            // Assert
            // ✅ Use EF Core query em vez de MongoDB
            var savedUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.UserName == "testuser");
            
            Assert.NotNull(savedUser);
            Assert.Equal("test@example.com", savedUser.Email);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnUser_WhenEmailExists()
        {
            // Arrange
            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
            };

            // ✅ Use EF Core para inserir
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var foundUser = await _userRepository.GetByEmailAsync("test@example.com");

            // Assert
            Assert.NotNull(foundUser);
            Assert.Equal("testuser", foundUser.UserName);
        }

        [Fact]
        public async Task AddToRoleAsync_ShouldAddUserToRole()
        {
            // Arrange
            var role = new ApplicationRole 
            { 
                Name = "Admin", 
                NormalizedName = "ADMIN" 
            };
            
            // ✅ Use EF Core
            _dbContext.Roles.Add(role);
            await _dbContext.SaveChangesAsync();

            var user = new ApplicationUser 
            { 
                UserName = "adminuser", 
                Email = "admin@example.com",
                Roles = new List<Guid>() // ✅ Inicializar lista
            };
            
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _userRepository.AddToRoleAsync(user, "Admin");

            // Assert
            Assert.True(result.Succeeded);

            // Verify the user role was added
            // ✅ Use EF Core query
            var updatedUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            Assert.NotNull(updatedUser.Roles);
            Assert.Contains(role.Id, updatedUser.Roles);
        }

        [Fact]
        public async Task IsInRoleAsync_ShouldReturnTrue_WhenUserIsInRole()
        {
            // Arrange
            var role = new ApplicationRole 
            { 
                Name = "User", 
                NormalizedName = "USER" 
            };
            
            _dbContext.Roles.Add(role);
            await _dbContext.SaveChangesAsync();

            var user = new ApplicationUser
            {
                UserName = "testuser",
                Email = "user@example.com",
                Roles = new List<Guid> { role.Id }, // ✅ Usar List<Guid> em vez de new()
            };
            
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var isInRole = await _userRepository.IsInRoleAsync(user, "User");

            // Assert
            Assert.True(isInRole);
        }

        // ✅ Teste adicional usando Specification
        [Fact]
        public async Task GetUsersByRole_ShouldReturnUsersInRole()
        {
            // Arrange
            var adminRole = new ApplicationRole { Name = "Admin", NormalizedName = "ADMIN" };
            var userRole = new ApplicationRole { Name = "User", NormalizedName = "USER" };
            
            _dbContext.Roles.AddRange(adminRole, userRole);
            await _dbContext.SaveChangesAsync();

            var adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@example.com",
                Roles = new List<Guid> { adminRole.Id }
            };

            var regularUser = new ApplicationUser
            {
                UserName = "user",
                Email = "user@example.com",
                Roles = new List<Guid> { userRole.Id }
            };

            _dbContext.Users.AddRange(adminUser, regularUser);
            await _dbContext.SaveChangesAsync();

            // Act - usando Specification (se você tiver uma)
            // var spec = new UsersByRoleSpecification("Admin");
            // var adminUsers = await _userRepository.ListAsync(spec);

            // Para demonstrar sem specification personalizada:
            var adminUsers = await _dbContext.Users
                .Where(u => u.Roles.Contains(adminRole.Id))
                .ToListAsync();

            // Assert
            Assert.Single(adminUsers);
            Assert.Equal("admin", adminUsers[0].UserName);
        }
    }
}
