using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTOs.Loans;
using LibraryManagement.Api.Models;
using LibraryManagement.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Tests.Services;

public class LoanServiceTests
{
    private LibraryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new LibraryDbContext(options);
    }

    private static Book CreateBook(bool available = true) => new()
    {
        Title = "Test Book",
        Author = "Test Author",
        ISBN = "123456789",
        PublicationYear = 2020,
        Available = available
    };

    private static Member CreateMember(bool isActive = true) => new()
    {
        FirstName = "Jane",
        LastName = "Doe",
        Email = "jane@test.com",
        MembershipDate = DateTime.UtcNow,
        IsActive = isActive
    };

    [Fact]
    public async Task BorrowBookAsync_HappyPath_CreatesLoanAndSetsBookUnavailable()
    {
        // Arrange
        using var context = CreateContext();
        context.Books.Add(CreateBook());
        context.Members.Add(CreateMember());
        await context.SaveChangesAsync();
        var service = new LoanService(context);
        var request = new CreateLoanRequest { BookId = 1, MemberId = 1 };

        // Act
        var result = await service.BorrowBookAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.False(result.Value!.IsReturned);
        Assert.Equal("Test Book", result.Value.BookTitle);
        Assert.Equal("Jane Doe", result.Value.MemberName);
        Assert.False(context.Books.First().Available);
    }

    [Fact]
    public async Task BorrowBookAsync_WhenBookDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var context = CreateContext();
        context.Members.Add(CreateMember());
        await context.SaveChangesAsync();
        var service = new LoanService(context);
        var request = new CreateLoanRequest { BookId = 999, MemberId = 1 };

        // Act
        var result = await service.BorrowBookAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ServiceErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task BorrowBookAsync_WhenMemberDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var context = CreateContext();
        context.Books.Add(CreateBook());
        await context.SaveChangesAsync();
        var service = new LoanService(context);
        var request = new CreateLoanRequest { BookId = 1, MemberId = 999 };

        // Act
        var result = await service.BorrowBookAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ServiceErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task BorrowBookAsync_WhenBookIsUnavailable_ReturnsBusinessRule()
    {
        // Arrange
        using var context = CreateContext();
        context.Books.Add(CreateBook(available: false));
        context.Members.Add(CreateMember());
        await context.SaveChangesAsync();
        var service = new LoanService(context);
        var request = new CreateLoanRequest { BookId = 1, MemberId = 1 };

        // Act
        var result = await service.BorrowBookAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ServiceErrorType.BusinessRule, result.ErrorType);
    }

    [Fact]
    public async Task BorrowBookAsync_WhenMemberIsInactive_ReturnsBusinessRule()
    {
        // Arrange
        using var context = CreateContext();
        context.Books.Add(CreateBook());
        context.Members.Add(CreateMember(isActive: false));
        await context.SaveChangesAsync();
        var service = new LoanService(context);
        var request = new CreateLoanRequest { BookId = 1, MemberId = 1 };

        // Act
        var result = await service.BorrowBookAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ServiceErrorType.BusinessRule, result.ErrorType);
    }

    [Fact]
    public async Task BorrowBookAsync_WhenMemberHasMaxLoans_ReturnsBusinessRule()
    {
        // Arrange
        using var context = CreateContext();
        context.Members.Add(CreateMember());

        // Add 4 books and create 3 active loans (max limit)
        for (int i = 0; i < 4; i++)
        {
            context.Books.Add(new Book { Title = $"Book {i}", Author = "Author", ISBN = $"ISBN{i}", PublicationYear = 2020, Available = i == 3 });
        }
        await context.SaveChangesAsync();

        context.Loans.AddRange(
            new Loan { BookId = 1, MemberId = 1, BorrowedDate = DateTime.UtcNow, IsReturned = false },
            new Loan { BookId = 2, MemberId = 1, BorrowedDate = DateTime.UtcNow, IsReturned = false },
            new Loan { BookId = 3, MemberId = 1, BorrowedDate = DateTime.UtcNow, IsReturned = false }
        );
        await context.SaveChangesAsync();
        var service = new LoanService(context);
        var request = new CreateLoanRequest { BookId = 4, MemberId = 1 };

        // Act
        var result = await service.BorrowBookAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ServiceErrorType.BusinessRule, result.ErrorType);
    }

    [Fact]
    public async Task ReturnBookAsync_HappyPath_SetsIsReturnedAndMakesBookAvailable()
    {
        // Arrange
        using var context = CreateContext();
        context.Books.Add(CreateBook(available: false));
        context.Members.Add(CreateMember());
        await context.SaveChangesAsync();
        context.Loans.Add(new Loan { BookId = 1, MemberId = 1, BorrowedDate = DateTime.UtcNow, IsReturned = false });
        await context.SaveChangesAsync();
        var service = new LoanService(context);

        // Act
        var result = await service.ReturnBookAsync(1);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Value!.IsReturned);
        Assert.NotNull(result.Value.ReturnedDate);
        Assert.True(context.Books.First().Available);
    }

    [Fact]
    public async Task ReturnBookAsync_WhenLoanDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var context = CreateContext();
        var service = new LoanService(context);

        // Act
        var result = await service.ReturnBookAsync(999);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ServiceErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task ReturnBookAsync_WhenAlreadyReturned_ReturnsBusinessRule()
    {
        // Arrange
        using var context = CreateContext();
        context.Books.Add(CreateBook());
        context.Members.Add(CreateMember());
        await context.SaveChangesAsync();
        context.Loans.Add(new Loan { BookId = 1, MemberId = 1, BorrowedDate = DateTime.UtcNow, ReturnedDate = DateTime.UtcNow, IsReturned = true });
        await context.SaveChangesAsync();
        var service = new LoanService(context);

        // Act
        var result = await service.ReturnBookAsync(1);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ServiceErrorType.BusinessRule, result.ErrorType);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllLoans()
    {
        // Arrange
        using var context = CreateContext();
        context.Books.Add(CreateBook());
        context.Members.Add(CreateMember());
        await context.SaveChangesAsync();
        context.Loans.Add(new Loan { BookId = 1, MemberId = 1, BorrowedDate = DateTime.UtcNow, IsReturned = false });
        await context.SaveChangesAsync();
        var service = new LoanService(context);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetByMemberAsync_ReturnsOnlyThatMembersLoans()
    {
        // Arrange
        using var context = CreateContext();
        context.Books.AddRange(CreateBook(), new Book { Title = "Book 2", Author = "Author", ISBN = "999", PublicationYear = 2020, Available = true });
        context.Members.AddRange(
            CreateMember(),
            new Member { FirstName = "John", LastName = "Smith", Email = "john@test.com", MembershipDate = DateTime.UtcNow, IsActive = true }
        );
        await context.SaveChangesAsync();
        context.Loans.AddRange(
            new Loan { BookId = 1, MemberId = 1, BorrowedDate = DateTime.UtcNow, IsReturned = false },
            new Loan { BookId = 2, MemberId = 2, BorrowedDate = DateTime.UtcNow, IsReturned = false }
        );
        await context.SaveChangesAsync();
        var service = new LoanService(context);

        // Act
        var result = await service.GetByMemberAsync(1);

        // Assert
        Assert.Single(result);
        Assert.All(result, l => Assert.Equal(1, l.MemberId));
    }
}
