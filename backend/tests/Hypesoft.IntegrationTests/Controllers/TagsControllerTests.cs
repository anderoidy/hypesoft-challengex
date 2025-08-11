using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Hypesoft.Application.Features.Tags.Commands;
using Hypesoft.Application.Features.Tags.Queries;
using Hypesoft.Domain.Entities;
using Xunit;

namespace Hypesoft.IntegrationTests.Controllers;

public class TagsControllerTests : TestBase, IAsyncLifetime
{
    private const string BaseUrl = "/api/tags";
    
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
    public async Task GetTags_ReturnsOk_WithTags()
    {
        // Arrange
        var tag1 = new Tag { Name = "Tag 1", Slug = "tag-1" };
        var tag2 = new Tag { Name = "Tag 2", Slug = "tag-2" };
        
        DbContext.Tags.AddRange(tag1, tag2);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await TestClient.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<Tag>>();
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(t => t.Name == "Tag 1");
        result.Items.Should().Contain(t => t.Name == "Tag 2");
    }

    [Fact]
    public async Task GetTagById_ReturnsTag_WhenTagExists()
    {
        // Arrange
        var tag = new Tag { Name = "Test Tag", Slug = "test-tag" };
        DbContext.Tags.Add(tag);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await TestClient.GetAsync($"{BaseUrl}/{tag.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Tag>();
        result.Should().NotBeNull();
        result.Name.Should().Be(tag.Name);
        result.Slug.Should().Be(tag.Slug);
    }

    [Fact]
    public async Task GetTagById_ReturnsNotFound_WhenTagDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await TestClient.GetAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTag_ReturnsCreated_WithValidData()
    {
        // Arrange
        var command = new CreateTagCommand { Name = "New Tag" };

        // Act
        var response = await TestClient.PostAsJsonAsync(BaseUrl, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdTag = await response.Content.ReadFromJsonAsync<Tag>();
        createdTag.Should().NotBeNull();
        createdTag.Name.Should().Be(command.Name);
        createdTag.Slug.Should().NotBeNullOrWhiteSpace();
        
        // Verify the tag was actually created in the database
        var dbTag = await DbContext.Tags.FindAsync(createdTag.Id);
        dbTag.Should().NotBeNull();
        dbTag.Name.Should().Be(command.Name);
    }

    [Fact]
    public async Task CreateTag_ReturnsBadRequest_WithInvalidData()
    {
        // Arrange - Name is required
        var command = new CreateTagCommand { Name = "" };

        // Act
        var response = await TestClient.PostAsJsonAsync(BaseUrl, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTag_ReturnsOk_WithValidData()
    {
        // Arrange
        var tag = new Tag { Name = "Old Name", Slug = "old-name" };
        DbContext.Tags.Add(tag);
        await DbContext.SaveChangesAsync();

        var command = new UpdateTagCommand 
        { 
            Id = tag.Id, 
            Name = "Updated Name" 
        };

        // Act
        var response = await TestClient.PutAsJsonAsync($"{BaseUrl}/{tag.Id}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedTag = await response.Content.ReadFromJsonAsync<Tag>();
        updatedTag.Should().NotBeNull();
        updatedTag.Name.Should().Be(command.Name);
        updatedTag.Slug.Should().NotBe(tag.Slug); // Slug should be regenerated
        
        // Verify the tag was actually updated in the database
        var dbTag = await DbContext.Tags.FindAsync(tag.Id);
        dbTag.Name.Should().Be(command.Name);
    }

    [Fact]
    public async Task UpdateTag_ReturnsBadRequest_WhenIdsDontMatch()
    {
        // Arrange
        var command = new UpdateTagCommand 
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
    public async Task DeleteTag_ReturnsNoContent_WhenTagExists()
    {
        // Arrange
        var tag = new Tag { Name = "To Delete", Slug = "to-delete" };
        DbContext.Tags.Add(tag);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await TestClient.DeleteAsync($"{BaseUrl}/{tag.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify the tag was actually deleted from the database
        var dbTag = await DbContext.Tags.FindAsync(tag.Id);
        dbTag.Should().BeNull();
    }

    [Fact]
    public async Task GetPopularTags_ReturnsOk_WithPopularTags()
    {
        // Arrange - Add some tags with different usage counts
        var popularTag = new Tag { Name = "Popular", Slug = "popular", UsageCount = 10 };
        var lessPopularTag = new Tag { Name = "Less Popular", Slug = "less-popular", UsageCount = 2 };
        var unusedTag = new Tag { Name = "Unused", Slug = "unused", UsageCount = 0 };
        
        DbContext.Tags.AddRange(popularTag, lessPopularTag, unusedTag);
        await DbContext.SaveChangesAsync();

        // Act - Request top 2 tags
        var response = await TestClient.GetAsync($"{BaseUrl}/popular?count=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Tag[]>();
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Name == "Popular");
        result.Should().Contain(t => t.Name == "Less Popular");
        result.Should().NotContain(t => t.Name == "Unused");
    }

    [Fact]
    public async Task GetTagBySlug_ReturnsTag_WhenSlugExists()
    {
        // Arrange
        var tag = new Tag { Name = "Test By Slug", Slug = "test-by-slug" };
        DbContext.Tags.Add(tag);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await TestClient.GetAsync($"{BaseUrl}/by-slug/{tag.Slug}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Tag>();
        result.Should().NotBeNull();
        result.Id.Should().Be(tag.Id);
        result.Name.Should().Be(tag.Name);
    }

    [Fact]
    public async Task GetTagBySlug_ReturnsNotFound_WhenSlugDoesNotExist()
    {
        // Act
        var response = await TestClient.GetAsync($"{BaseUrl}/by-slug/non-existent-slug");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
