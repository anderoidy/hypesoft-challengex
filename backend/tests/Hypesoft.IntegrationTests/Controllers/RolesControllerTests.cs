using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Hypesoft.Application.Features.Roles.Commands;
using Hypesoft.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace Hypesoft.IntegrationTests.Controllers;

public class RolesControllerTests : TestBase, IAsyncLifetime
{
    private const string BaseUrl = "/api/roles";
    
    public async Task InitializeAsync()
    {
        // Autentica o usuário de teste antes de cada teste
        await AuthenticateAsync();
    }

    public Task DisposeAsync()
    {
        // Limpeza é feita pela classe base
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetRoles_ReturnsOk_WithRoles()
    {
        // Arrange
        var role1 = new ApplicationRole { Name = "Test Role 1", NormalizedName = "TEST ROLE 1" };
        var role2 = new ApplicationRole { Name = "Test Role 2", NormalizedName = "TEST ROLE 2" };
        
        DbContext.Roles.AddRange(role1, role2);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await TestClient.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<ApplicationRole>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Name == "Test Role 1");
        result.Should().Contain(r => r.Name == "Test Role 2");
    }

    [Fact]
    public async Task GetRoleById_ReturnsRole_WhenRoleExists()
    {
        // Arrange
        var role = new ApplicationRole { Name = "Test Role", NormalizedName = "TEST ROLE" };
        DbContext.Roles.Add(role);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await TestClient.GetAsync($"{BaseUrl}/{role.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApplicationRole>();
        result.Should().NotBeNull();
        result.Name.Should().Be(role.Name);
        result.NormalizedName.Should().Be(role.NormalizedName);
    }

    [Fact]
    public async Task GetRoleById_ReturnsNotFound_WhenRoleDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await TestClient.GetAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRoleByName_ReturnsRole_WhenNameExists()
    {
        // Arrange
        var role = new ApplicationRole { Name = "Test Role", NormalizedName = "TEST ROLE" };
        DbContext.Roles.Add(role);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await TestClient.GetAsync($"{BaseUrl}/byname/{role.Name}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApplicationRole>();
        result.Should().NotBeNull();
        result.Id.Should().Be(role.Id);
        result.Name.Should().Be(role.Name);
    }

    [Fact]
    public async Task GetRoleByName_ReturnsNotFound_WhenNameDoesNotExist()
    {
        // Act
        var response = await TestClient.GetAsync($"{BaseUrl}/byname/nonexistent-role");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateRole_ReturnsCreated_WithValidData()
    {
        // Arrange
        var command = new CreateRoleCommand { Name = "New Role" };

        // Act
        var response = await TestClient.PostAsJsonAsync(BaseUrl, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdRole = await response.Content.ReadFromJsonAsync<ApplicationRole>();
        createdRole.Should().NotBeNull();
        createdRole.Name.Should().Be(command.Name);
        
        // Verify the role was actually created in the database
        var dbRole = await DbContext.Roles.FindAsync(createdRole.Id);
        dbRole.Should().NotBeNull();
        dbRole.Name.Should().Be(command.Name);
    }

    [Fact]
    public async Task CreateRole_ReturnsBadRequest_WithInvalidData()
    {
        // Arrange - Name is required
        var command = new CreateRoleCommand { Name = "" };

        // Act
        var response = await TestClient.PostAsJsonAsync(BaseUrl, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateRole_ReturnsOk_WithValidData()
    {
        // Arrange
        var role = new ApplicationRole { Name = "Old Name", NormalizedName = "OLD NAME" };
        DbContext.Roles.Add(role);
        await DbContext.SaveChangesAsync();

        var command = new UpdateRoleCommand 
        { 
            Id = role.Id, 
            Name = "Updated Name" 
        };

        // Act
        var response = await TestClient.PutAsJsonAsync($"{BaseUrl}/{role.Id}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedRole = await response.Content.ReadFromJsonAsync<ApplicationRole>();
        updatedRole.Should().NotBeNull();
        updatedRole.Name.Should().Be(command.Name);
        updatedRole.NormalizedName.Should().Be("UPDATED NAME");
        
        // Verify the role was actually updated in the database
        var dbRole = await DbContext.Roles.FindAsync(role.Id);
        dbRole.Name.Should().Be(command.Name);
        dbRole.NormalizedName.Should().Be("UPDATED NAME");
    }

    [Fact]
    public async Task UpdateRole_ReturnsBadRequest_WhenIdsDontMatch()
    {
        // Arrange
        var command = new UpdateRoleCommand 
        { 
            Id = Guid.NewGuid(), 
            Name = "Updated Name" 
        };

        // Act - Different ID in URL and command
        var response = await TestClient.PutAsJsonAsync($"{BaseUrl}/{Guid.NewGuid()}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteRole_ReturnsNoContent_WhenRoleExists()
    {
        // Arrange
        var role = new ApplicationRole { Name = "Role to Delete", NormalizedName = "ROLE TO DELETE" };
        DbContext.Roles.Add(role);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await TestClient.DeleteAsync($"{BaseUrl}/{role.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the role was actually deleted from the database
        var dbRole = await DbContext.Roles.FindAsync(role.Id);
        dbRole.Should().BeNull();
    }

    [Fact]
    public async Task AddClaimToRole_ReturnsNoContent_WhenValid()
    {
        // Arrange
        var role = new ApplicationRole { Name = "Role with Claim", NormalizedName = "ROLE WITH CLAIM" };
        DbContext.Roles.Add(role);
        await DbContext.SaveChangesAsync();

        var command = new AddClaimToRoleCommand
        {
            RoleId = role.Id,
            ClaimType = "Permission",
            ClaimValue = "CanDoSomething"
        };

        // Act
        var response = await TestClient.PostAsJsonAsync($"{BaseUrl}/{role.Id}/claims", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the claim was added to the role
        var dbRole = await DbContext.Roles.FindAsync(role.Id);
        dbRole.Claims.Should().NotBeEmpty();
        dbRole.Claims.Should().Contain(c => 
            c.ClaimType == "Permission" && 
            c.ClaimValue == "CanDoSomething");
    }

    [Fact]
    public async Task RemoveClaimFromRole_ReturnsNoContent_WhenValid()
    {
        // Arrange
        var role = new ApplicationRole { Name = "Role with Claim", NormalizedName = "ROLE WITH CLAIM" };
        role.AddClaim("Permission", "CanDoSomething");
        
        DbContext.Roles.Add(role);
        await DbContext.SaveChangesAsync();

        var command = new RemoveClaimFromRoleCommand
        {
            RoleId = role.Id,
            ClaimType = "Permission",
            ClaimValue = "CanDoSomething"
        };

        // Act
        var response = await TestClient.DeleteAsJsonAsync($"{BaseUrl}/{role.Id}/claims", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the claim was removed from the role
        var dbRole = await DbContext.Roles.FindAsync(role.Id);
        dbRole.Claims.Should().BeEmpty();
    }
}
