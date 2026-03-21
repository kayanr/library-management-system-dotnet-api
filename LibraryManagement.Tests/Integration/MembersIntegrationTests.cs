using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTOs.Members;
using LibraryManagement.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.Tests.Integration;

public class MembersIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MembersIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<LibraryDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                var dbName = "MembersIntegrationTestDb_" + Guid.NewGuid();
                services.AddDbContext<LibraryDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));
            });
        }).CreateClient();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtHelper.GenerateToken());
    }

    [Fact]
    public async Task GetMembers_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/members");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateMember_WithValidData_ReturnsCreated()
    {
        var request = new CreateMemberRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@test.com",
            Phone = "555-1234"
        };

        var response = await _client.PostAsJsonAsync("/api/members", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var member = await response.Content.ReadFromJsonAsync<MemberResponse>();
        Assert.NotNull(member);
        Assert.Equal("Jane", member.FirstName);
        Assert.True(member.IsActive);
    }

    [Fact]
    public async Task CreateMember_WithDuplicateEmail_ReturnsConflict()
    {
        var request = new CreateMemberRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "duplicate@test.com"
        };

        await _client.PostAsJsonAsync("/api/members", request);
        var response = await _client.PostAsJsonAsync("/api/members", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetMember_WhenMemberDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/members/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMember_WhenMemberDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/members/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
