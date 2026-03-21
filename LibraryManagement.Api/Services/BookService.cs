using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTOs.Books;
using LibraryManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Services;

public class BookService : IBookService
{
    private readonly LibraryDbContext _context;

    public BookService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BookResponse>> GetAllAsync()
    {
        return await _context.Books
            .Select(b => MapToResponse(b))
            .ToListAsync();
    }

    public async Task<BookResponse?> GetByIdAsync(int id)
    {
        var book = await _context.Books.FindAsync(id);
        return book == null ? null : MapToResponse(book);
    }

    public async Task<BookResponse> CreateAsync(CreateBookRequest request)
    {
        var book = new Book
        {
            Title = request.Title,
            Author = request.Author,
            ISBN = request.ISBN,
            PublicationYear = request.PublicationYear,
            Available = true
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        return MapToResponse(book);
    }

    public async Task<ServiceResult<BookResponse>> UpdateAsync(int id, UpdateBookRequest request)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
            return ServiceResult<BookResponse>.Fail("Book not found.", ServiceErrorType.NotFound);

        book.Title = request.Title;
        book.Author = request.Author;
        book.ISBN = request.ISBN;
        book.PublicationYear = request.PublicationYear;
        book.Available = request.Available;

        await _context.SaveChangesAsync();

        return ServiceResult<BookResponse>.Ok(MapToResponse(book));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
            return ServiceResult<bool>.Fail("Book not found.", ServiceErrorType.NotFound);

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    private static BookResponse MapToResponse(Book book) => new()
    {
        Id = book.Id,
        Title = book.Title,
        Author = book.Author,
        ISBN = book.ISBN,
        PublicationYear = book.PublicationYear,
        Available = book.Available
    };
}
