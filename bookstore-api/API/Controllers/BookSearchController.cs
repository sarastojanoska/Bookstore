using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = "BooksSearch")]
    [ApiController]
    public class BookSearchController(IBookService bookService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<PagedResult<BookDto>>> Search(
            [FromQuery] string? title,
            [FromQuery] string? authorName,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await bookService.SearchAsync(
                    title,
                    authorName,
                    pageNumber,
                    pageSize,
                    cancellationToken);

                return Ok(result);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
