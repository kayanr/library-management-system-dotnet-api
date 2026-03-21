using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTOs.Members;
using LibraryManagement.Api.Models;
using LibraryManagement.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Tests.Services;

public class MemberServiceTests
{
    private LibraryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new LibraryDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllMembers()
    {
        // Arrange
        using var context = CreateContext();
        context.Members.AddRange(
            new Member { FirstName = "Jane", LastName = "Doe", Email = "jane@test.com", MembershipDate = DateTime.UtcNow, IsActive = true },
            new Member { FirstName = "John", LastName = "Smith", Email = "john@test.com", MembershipDate = DateTime.UtcNow, IsActive = true }
        );
        await context.SaveChangesAsync();
        var service = new MemberService(context);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_WhenMemberExists_ReturnsSuccess()
    {
        // Arrange
        using var context = CreateContext();
        context.Members.Add(new Member { FirstName = "Jane", LastName = "Doe", Email = "jane@test.com", MembershipDate = DateTime.UtcNow, IsActive = true });
        await context.SaveChangesAsync();
        var service = new MemberService(context);

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Jane", result.Value!.FirstName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMemberDoesNotExist_ReturnsFailure()
    {
        // Arrange
        using var context = CreateContext();
        var service = new MemberService(context);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ServiceErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task CreateAsync_WithUniqueEmail_CreatesAndReturnsMember()
    {
        // Arrange
        using var context = CreateContext();
        var service = new MemberService(context);
        var request = new CreateMemberRequest
        {
            FirstName = "Alex",
            LastName = "Brown",
            Email = "alex@test.com",
            Phone = "555-1234"
        };

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Alex", result.Value!.FirstName);
        Assert.True(result.Value.IsActive);
        Assert.Equal(1, context.Members.Count());
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ReturnsConflict()
    {
        // Arrange
        using var context = CreateContext();
        context.Members.Add(new Member { FirstName = "Jane", LastName = "Doe", Email = "jane@test.com", MembershipDate = DateTime.UtcNow, IsActive = true });
        await context.SaveChangesAsync();
        var service = new MemberService(context);
        var request = new CreateMemberRequest
        {
            FirstName = "Janet",
            LastName = "Doe",
            Email = "jane@test.com"
        };

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ServiceErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task UpdateAsync_WhenMemberDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var context = CreateContext();
        var service = new MemberService(context);
        var request = new UpdateMemberRequest { FirstName = "Jane", LastName = "Doe", Email = "jane@test.com", IsActive = true };

        // Act
        var result = await service.UpdateAsync(999, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ServiceErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task UpdateAsync_WithEmailTakenByAnotherMember_ReturnsConflict()
    {
        // Arrange
        using var context = CreateContext();
        context.Members.AddRange(
            new Member { FirstName = "Jane", LastName = "Doe", Email = "jane@test.com", MembershipDate = DateTime.UtcNow, IsActive = true },
            new Member { FirstName = "John", LastName = "Smith", Email = "john@test.com", MembershipDate = DateTime.UtcNow, IsActive = true }
        );
        await context.SaveChangesAsync();
        var service = new MemberService(context);
        var request = new UpdateMemberRequest { FirstName = "Jane", LastName = "Doe", Email = "john@test.com", IsActive = true };

        // Act
        var result = await service.UpdateAsync(1, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ServiceErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public async Task DeleteAsync_WhenMemberExists_DeletesAndReturnsSuccess()
    {
        // Arrange
        using var context = CreateContext();
        context.Members.Add(new Member { FirstName = "Jane", LastName = "Doe", Email = "jane@test.com", MembershipDate = DateTime.UtcNow, IsActive = true });
        await context.SaveChangesAsync();
        var service = new MemberService(context);

        // Act
        var result = await service.DeleteAsync(1);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, context.Members.Count());
    }

    [Fact]
    public async Task DeleteAsync_WhenMemberDoesNotExist_ReturnsFailure()
    {
        // Arrange
        using var context = CreateContext();
        var service = new MemberService(context);

        // Act
        var result = await service.DeleteAsync(999);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(ServiceErrorType.NotFound, result.ErrorType);
    }
}
