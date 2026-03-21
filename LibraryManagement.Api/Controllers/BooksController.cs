using LibraryManagement.Api.DTOs.Books;
using LibraryManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    // GET: api/books
    [HttpGet]
    public async Task<IActionResult> GetBooks()
    {
        var books = await _bookService.GetAllAsync();
        return Ok(books);
    }

    // GET: api/books/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBook(int id)
    {
        var book = await _bookService.GetByIdAsync(id);

        if (book == null)
            return NotFound();

        return Ok(book);
    }

    // POST: api/books
    [HttpPost]
    public async Task<IActionResult> CreateBook(CreateBookRequest request)
    {
        var book = await _bookService.CreateAsync(request);
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    // PUT: api/books/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBook(int id, UpdateBookRequest request)
    {
        var result = await _bookService.UpdateAsync(id, request);

        if (!result.Success)
            return MapServiceError(result.ErrorType, result.ErrorMessage!);

        return Ok(result.Value);
    }

    // DELETE: api/books/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var result = await _bookService.DeleteAsync(id);

        if (!result.Success)
            return MapServiceError(result.ErrorType, result.ErrorMessage!);

        return NoContent();
    }

    private IActionResult MapServiceError(ServiceErrorType? errorType, string message) =>
        errorType switch
        {
            ServiceErrorType.NotFound => NotFound(new { message }),
            ServiceErrorType.Conflict => Conflict(new { message }),
            ServiceErrorType.BusinessRule => UnprocessableEntity(new { message }),
            ServiceErrorType.Validation => BadRequest(new { message }),
            _ => StatusCode(500, new { message })
        };
}
