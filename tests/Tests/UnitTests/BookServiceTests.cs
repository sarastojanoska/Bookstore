using Application.DTOs;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Services.BookService;

namespace Tests.UnitTests
{
    [TestFixture]
    public class BookServiceTests
    {
        private AppDbContext _context = null!;
        private BookService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _sut = new BookService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        private Author SeedAuthor(string name = "Jane Austen")
        {
            var author = new Author { Name = name };
            _context.Authors.Add(author);
            _context.SaveChanges();

            return author;
        }

        private Book SeedBook(int authorId, string title = "Pride and Prejudice", string? subTitle = null)
        {
            var book = new Book { AuthorId = authorId, Title = title, SubTitle = subTitle };
            _context.Books.Add(book);
            _context.SaveChanges();

            return book;
        }

        [Test]
        public async Task GetAllAsync_ReturnsAllBooks()
        {
            var author = SeedAuthor();
            SeedBook(author.Id, "Book One");
            SeedBook(author.Id, "Book Two");

            var result = await _sut.GetAllAsync(CancellationToken.None);

            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllAsync_WhenNoBooks_ReturnsEmptyCollection()
        {
            var result = await _sut.GetAllAsync(CancellationToken.None);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetByIdAsync_WhenBookExists_ReturnsBook()
        {
            var author = SeedAuthor();
            var book = SeedBook(author.Id);

            var result = await _sut.GetByIdAsync(book.Id, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(book.Id));
            Assert.That(result.Title, Is.EqualTo(book.Title));
        }

        [Test]
        public async Task GetByIdAsync_WhenBookDoesNotExist_ReturnsNull()
        {
            var result = await _sut.GetByIdAsync(999, CancellationToken.None);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task CreateAsync_WhenAuthorExists_CreatesBook()
        {
            var author = SeedAuthor();
            var dto = new CreateBookDto { AuthorId = author.Id, Title = "New Book", SubTitle = "A Subtitle" };

            var result = await _sut.CreateAsync(dto, CancellationToken.None);

            Assert.That(result.Id, Is.GreaterThan(0));
            Assert.That(result.Title, Is.EqualTo(dto.Title));
            Assert.That(result.SubTitle, Is.EqualTo(dto.SubTitle));
            Assert.That(await _context.Books.CountAsync(), Is.EqualTo(1));
        }

        [Test]
        public void CreateAsync_WhenAuthorDoesNotExist_ThrowsArgumentException()
        {
            var dto = new CreateBookDto { AuthorId = 999, Title = "New Book" };

            Assert.ThrowsAsync<ArgumentException>(
                async () => await _sut.CreateAsync(dto, CancellationToken.None));
        }

        [Test]
        public async Task UpdateAsync_WhenBookAndAuthorExist_UpdatesBookAndReturnsTrue()
        {
            var author = SeedAuthor();
            var book = SeedBook(author.Id);
            var dto = new UpdateBookDto { AuthorId = author.Id, Title = "Updated Title", SubTitle = "Updated Subtitle" };

            var result = await _sut.UpdateAsync(book.Id, dto, CancellationToken.None);

            Assert.That(result, Is.True);

            var updated = await _context.Books.FindAsync(book.Id);
            Assert.That(updated!.Title, Is.EqualTo(dto.Title));
            Assert.That(updated.SubTitle, Is.EqualTo(dto.SubTitle));
        }

        [Test]
        public async Task UpdateAsync_WhenBookDoesNotExist_ReturnsFalse()
        {
            var author = SeedAuthor();
            var dto = new UpdateBookDto { AuthorId = author.Id, Title = "Updated Title" };

            var result = await _sut.UpdateAsync(999, dto, CancellationToken.None);

            Assert.That(result, Is.False);
        }

        [Test]
        public void UpdateAsync_WhenAuthorDoesNotExist_ThrowsArgumentException()
        {
            var author = SeedAuthor();
            var book = SeedBook(author.Id);
            var dto = new UpdateBookDto { AuthorId = 999, Title = "Updated Title" };

            Assert.ThrowsAsync<ArgumentException>(
                async () => await _sut.UpdateAsync(book.Id, dto, CancellationToken.None));
        }

        [Test]
        public async Task DeleteAsync_WhenBookExists_DeletesBookAndReturnsTrue()
        {
            var author = SeedAuthor();
            var book = SeedBook(author.Id);

            var result = await _sut.DeleteAsync(book.Id, CancellationToken.None);

            Assert.That(result, Is.True);
            Assert.That(await _context.Books.CountAsync(), Is.EqualTo(0));
        }

        [Test]
        public async Task DeleteAsync_WhenBookDoesNotExist_ReturnsFalse()
        {
            var result = await _sut.DeleteAsync(999, CancellationToken.None);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task SearchAsync_FiltersByTitle()
        {
            var author = SeedAuthor();
            SeedBook(author.Id, "Pride and Prejudice");
            SeedBook(author.Id, "Emma");

            var result = await _sut.SearchAsync("Pride", null, 1, 10, CancellationToken.None);

            Assert.That(result.Items, Has.Count.EqualTo(1));
            Assert.That(result.Items[0].Title, Is.EqualTo("Pride and Prejudice"));
        }

        [Test]
        public async Task SearchAsync_FiltersByAuthorName()
        {
            var author1 = SeedAuthor("Jane Austen");
            var author2 = SeedAuthor("Mark Twain");
            SeedBook(author1.Id, "Pride and Prejudice");
            SeedBook(author2.Id, "Tom Sawyer");

            var result = await _sut.SearchAsync(null, "Twain", 1, 10, CancellationToken.None);

            Assert.That(result.Items, Has.Count.EqualTo(1));
            Assert.That(result.Items[0].Title, Is.EqualTo("Tom Sawyer"));
        }

        [Test]
        public async Task SearchAsync_ReturnsPagedResults()
        {
            var author = SeedAuthor();
            for (var i = 1; i <= 5; i++)
            {
                SeedBook(author.Id, $"Book {i}");
            }

            var result = await _sut.SearchAsync(null, null, 2, 2, CancellationToken.None);

            Assert.That(result.Items, Has.Count.EqualTo(2));
            Assert.That(result.TotalCount, Is.EqualTo(5));
            Assert.That(result.PageNumber, Is.EqualTo(2));
            Assert.That(result.PageSize, Is.EqualTo(2));
            Assert.That(result.TotalPages, Is.EqualTo(3));
        }

        [Test]
        public void SearchAsync_WhenPageNumberIsInvalid_ThrowsArgumentOutOfRangeException()
        {
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await _sut.SearchAsync(null, null, 0, 10, CancellationToken.None));
        }

        [Test]
        public void SearchAsync_WhenPageSizeIsInvalid_ThrowsArgumentOutOfRangeException()
        {
            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await _sut.SearchAsync(null, null, 1, 101, CancellationToken.None));
        }
    }
}
