using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Services.BookService
{
    public class BookService(AppDbContext context) : IBookService
    {
        public async Task<IEnumerable<BookDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await context.Books
                .AsNoTracking()
                .Select(book => new BookDto
                {
                    Id = book.Id,
                    AuthorId = book.AuthorId,
                    Title = book.Title,
                    SubTitle = book.SubTitle
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<BookDto?> GetByIdAsync(int id,CancellationToken cancellationToken)
        {
            return await context.Books
                .AsNoTracking()
                .Where(book => book.Id == id)
                .Select(book => new BookDto
                {
                    Id = book.Id,
                    AuthorId = book.AuthorId,
                    Title = book.Title,
                    SubTitle = book.SubTitle
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<BookDto> CreateAsync(CreateBookDto dto,CancellationToken cancellationToken)
        {
            var authorExists = await context.Authors
                .AnyAsync(author => author.Id == dto.AuthorId,
                    cancellationToken);

            if (!authorExists)
            {
                throw new ArgumentException(
                    $"Author with ID {dto.AuthorId} does not exist.");
            }

            var book = new Book
            {
                AuthorId = dto.AuthorId,
                Title = dto.Title,
                SubTitle = dto.SubTitle
            };

            context.Books.Add(book);

            await context.SaveChangesAsync(cancellationToken);

            return new BookDto
            {
                Id = book.Id,
                AuthorId = book.AuthorId,
                Title = book.Title,
                SubTitle = book.SubTitle
            };
        }

        public async Task<bool> UpdateAsync(int id,UpdateBookDto dto,CancellationToken cancellationToken)
        {
            var book = await context.Books
                .FirstOrDefaultAsync(book => book.Id == id,
                    cancellationToken);

            if (book is null)
            {
                return false;
            }

            var authorExists = await context.Authors
                .AnyAsync(author => author.Id == dto.AuthorId,
                    cancellationToken);

            if (!authorExists)
            {
                throw new ArgumentException(
                    $"Author with ID {dto.AuthorId} does not exist.");
            }

            book.AuthorId = dto.AuthorId;
            book.Title = dto.Title;
            book.SubTitle = dto.SubTitle;

            await context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<bool> DeleteAsync(int id,CancellationToken cancellationToken)
        {
            var book = await context.Books
                .FirstOrDefaultAsync(book => book.Id == id,
                    cancellationToken);

            if (book is null)
            {
                return false;
            }

            context.Books.Remove(book);

            await context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<PagedResult<BookDto>> SearchAsync(string? title,string? authorName,int pageNumber,int pageSize,CancellationToken cancellationToken)
        {
            if (pageNumber < 1)
                throw new ArgumentOutOfRangeException(
                    nameof(pageNumber), "Page number must be greater than 0.");

            if (pageSize < 1 || pageSize > 100)
                throw new ArgumentOutOfRangeException(
                    nameof(pageSize), "Page size must be between 1 and 100.");


            var query = context.Books
                .AsNoTracking()
                .Join(
                    context.Authors.AsNoTracking(),
                    book => book.AuthorId,
                    author => author.Id,
                    (book, author) => new
                    {
                        Book = book,
                        AuthorName = author.Name
                    });

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(x =>
                    x.Book.Title.Contains(title));
            }

            if (!string.IsNullOrWhiteSpace(authorName))
            {
                query = query.Where(x =>
                    x.AuthorName.Contains(authorName));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(x => x.Book.Title)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BookDto
                {
                    Id = x.Book.Id,
                    AuthorId = x.Book.AuthorId,
                    Title = x.Book.Title,
                    SubTitle = x.Book.SubTitle
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<BookDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

    }
}
