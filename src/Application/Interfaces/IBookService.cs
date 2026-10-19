using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<BookDto>> GetAllAsync(CancellationToken cancellationToken);

        Task<BookDto?> GetByIdAsync(int id, CancellationToken cancellationToken);

        Task<BookDto> CreateAsync(CreateBookDto dto, CancellationToken cancellationToken);

        Task<bool> UpdateAsync(int id, UpdateBookDto dto, CancellationToken cancellationToken);

        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);

        Task<PagedResult<BookDto>> SearchAsync(string? title,string? authorName,int pageNumber,int pageSize,CancellationToken cancellationToken);
    }
}
