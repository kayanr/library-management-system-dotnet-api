using LibraryManagement.Api.DTOs.Members;
using LibraryManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;

    public MembersController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var members = await _memberService.GetAllAsync();
        return Ok(members);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _memberService.GetByIdAsync(id);
        if (!result.Success)
            return MapServiceError(result.ErrorType, result.ErrorMessage!);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMemberRequest request)
    {
        var result = await _memberService.CreateAsync(request);
        if (!result.Success)
            return MapServiceError(result.ErrorType, result.ErrorMessage!);

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMemberRequest request)
    {
        var result = await _memberService.UpdateAsync(id, request);
        if (!result.Success)
            return MapServiceError(result.ErrorType, result.ErrorMessage!);

        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _memberService.DeleteAsync(id);
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
