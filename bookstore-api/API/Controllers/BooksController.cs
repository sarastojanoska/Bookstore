using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController(IBookService bookService) : ControllerBase
    {

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetAll(CancellationToken cancellationToken)
        {
            var books = await bookService.GetAllAsync(cancellationToken);

            return Ok(books);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var book = await bookService.GetByIdAsync(
                id, cancellationToken);

            if (book is null)
                return NotFound();

            return Ok(book);
        }

        [HttpPost]
        public async Task<ActionResult<BookDto>> Create([FromBody] CreateBookDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var book = await bookService.CreateAsync(
                    dto, cancellationToken);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = book.Id },
                    book);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateBookDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var updated = await bookService.UpdateAsync(
                    id, dto, cancellationToken);

                if (!updated)
                    return NotFound();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var deleted = await bookService.DeleteAsync(
                id, cancellationToken);

            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<BookDto>>> Search([FromQuery] string? title, [FromQuery] string? authorName, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
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
