using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTOs.Books;
using LibraryManagement.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Tests.Services;

public class BookServiceTests
{
    private LibraryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new LibraryDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllBooks()
    {
        // Arrange
        using var context = CreateContext();
        context.Books.AddRange(
            new Api.Models.Book { Title = "Book A", Author = "Author A", ISBN = "111", PublicationYear = 2000, Available = true },
            new Api.Models.Book { Title = "Book B", Author = "Author B", ISBN = "222", PublicationYear = 2001, Available = true }
        );
        await context.SaveChangesAsync();
        var service = new BookService(context);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookExists_ReturnsBook()
    {
        // Arrange
        using var context = CreateContext();
        context.Books.Add(new Api.Models.Book { Title = "Clean Code", Author = "Martin", ISBN = "123", PublicationYear = 2008, Available = true });
        await context.SaveChangesAsync();
        var service = new BookService(context);

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Clean Code", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookDoesNotExist_ReturnsNull()
    {
        // Arrange
        using var context = CreateContext();
        var service = new BookService(context);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_CreatesBookWithAvailableTrue()
    {
        // Arrange
        using var context = CreateContext();
        var service = new BookService(context);
        var request = new CreateBookRequest
        {
            Title = "New Book",
            Author = "New Author",
            ISBN = "999",
            PublicationYear = 2024
        };

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.Equal("New Book", result.Title);
        Assert.True(result.Available);
        Assert.Equal(1, context.Books.Count());
    }

    [Fact]
    public async Task UpdateAsync_WhenBookExists_UpdatesAndReturnsBook()
    {
        // Arrange
        using var context = CreateContext();
        context.Books.Add(new Api.Models.Book { Title = "Old Title", Author = "Author", ISBN = "123", PublicationYear = 2000, Available = true });
        await context.SaveChangesAsync();
        var service = new BookService(context);
        var request = new UpdateBookRequest { Title = "New Title", Author = "Author", ISBN = "123", PublicationYear = 2000, Available = true };

        // Act
        var result = await service.UpdateAsync(1, request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("New Title", result.Value!.Title);
    }

    [Fact]
    public async Task UpdateAsync_WhenBookDoesNotExist_ReturnsFailure()
    {
        // Arrange
        using var context = CreateContext();
        var service = new BookService(context);
        var request = new UpdateBookRequest { Title = "Title", Author = "Author", ISBN = "123", PublicationYear = 2000, Available = true };

        // Act
        var result = await service.UpdateAsync(999, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Book not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteAsync_WhenBookExists_DeletesAndReturnsSuccess()
    {
        // Arrange
        using var context = CreateContext();
        context.Books.Add(new Api.Models.Book { Title = "Book", Author = "Author", ISBN = "123", PublicationYear = 2000, Available = true });
        await context.SaveChangesAsync();
        var service = new BookService(context);

        // Act
        var result = await service.DeleteAsync(1);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, context.Books.Count());
    }

    [Fact]
    public async Task DeleteAsync_WhenBookDoesNotExist_ReturnsFailure()
    {
        // Arrange
        using var context = CreateContext();
        var service = new BookService(context);

        // Act
        var result = await service.DeleteAsync(999);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Book not found.", result.ErrorMessage);
    }
}
