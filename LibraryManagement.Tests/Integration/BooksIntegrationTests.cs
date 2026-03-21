using System.Net;
using System.Net.Http.Json;
using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTOs.Books;
using LibraryManagement.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.Tests.Integration;

public class BooksIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public BooksIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real SQLite DbContext
                var descriptor = services.SingleOrDefault(d =>
                    d.ServiceType == typeof(DbContextOptions<LibraryDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Replace with in-memory database
                var dbName = "BooksIntegrationTestDb_" + Guid.NewGuid();
                services.AddDbContext<LibraryDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetBooks_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/books");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateBook_WithValidData_ReturnsCreated()
    {
        var request = new CreateBookRequest
        {
            Title = "Integration Test Book",
            Author = "Test Author",
            ISBN = "9991112223334",
            PublicationYear = 2024
        };

        var response = await _client.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var book = await response.Content.ReadFromJsonAsync<BookResponse>();
        Assert.NotNull(book);
        Assert.Equal("Integration Test Book", book.Title);
        Assert.True(book.Available);
    }

    [Fact]
    public async Task CreateBook_WithMissingFields_ReturnsBadRequest()
    {
        var request = new { PublicationYear = 2024 };

        var response = await _client.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetBook_WhenBookDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/books/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_WhenBookDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/books/999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
