using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTOs.Loans;
using LibraryManagement.Api.Models;
using LibraryManagement.Tests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.Tests.Integration;

public class LoansIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public LoansIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<LibraryDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                var dbName = "LoansIntegrationTestDb_" + Guid.NewGuid();
                services.AddDbContext<LibraryDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));
            });
        });
        _client = _factory.CreateClient();

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TestJwtHelper.GenerateToken());
    }

    private async Task SeedDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        context.Books.Add(new Book { Title = "Test Book", Author = "Author", ISBN = "123", PublicationYear = 2020, Available = true });
        context.Members.Add(new Member { FirstName = "Jane", LastName = "Doe", Email = "jane@test.com", MembershipDate = DateTime.UtcNow, IsActive = true });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetLoans_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/loans");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task BorrowBook_WithValidData_ReturnsCreated()
    {
        await SeedDataAsync();
        var request = new CreateLoanRequest { BookId = 1, MemberId = 1 };

        var response = await _client.PostAsJsonAsync("/api/loans/borrow", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var loan = await response.Content.ReadFromJsonAsync<LoanResponse>();
        Assert.NotNull(loan);
        Assert.False(loan.IsReturned);
        Assert.Equal("Test Book", loan.BookTitle);
    }

    [Fact]
    public async Task BorrowBook_WithNonExistentBook_ReturnsNotFound()
    {
        var request = new CreateLoanRequest { BookId = 999, MemberId = 1 };
        var response = await _client.PostAsJsonAsync("/api/loans/borrow", request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ReturnBook_WhenLoanDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync("/api/loans/return/999", new {});
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
