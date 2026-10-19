using System.Net;
using System.Net.Http.Json;
using Application.DTOs;
using Domain.Entities;
using Infrastructure.Data;

namespace Tests.IntegrationTests
{
    [TestFixture]
    public class BooksControllerIntegrationTests
    {
        private CustomWebApplicationFactory _factory = null!;
        private HttpClient _client = null!;

        [SetUp]
        public void SetUp()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        private void AuthorizeAsCrudClient()
        {
            _client.DefaultRequestHeaders.Add(TestAuthHandler.ScopeHeader, "books.crud");
        }

        private Author SeedAuthor(string name = "Jane Austen")
        {
            Author author = null!;

            _factory.SeedDatabase(context =>
            {
                author = new Author { Name = name };
                context.Authors.Add(author);
            });

            return author;
        }

        private Book SeedBook(int authorId, string title = "Pride and Prejudice")
        {
            Book book = null!;

            _factory.SeedDatabase(context =>
            {
                book = new Book { AuthorId = authorId, Title = title };
                context.Books.Add(book);
            });

            return book;
        }

        [Test]
        public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/api/books");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task GetAll_WhenAuthorized_ReturnsAllBooks()
        {
            AuthorizeAsCrudClient();
            var author = SeedAuthor();
            SeedBook(author.Id, "Book One");
            SeedBook(author.Id, "Book Two");

            var response = await _client.GetAsync("/api/books");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var books = await response.Content.ReadFromJsonAsync<List<BookDto>>();
            Assert.That(books, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task GetById_WhenBookExists_ReturnsBook()
        {
            AuthorizeAsCrudClient();
            var author = SeedAuthor();
            var book = SeedBook(author.Id);

            var response = await _client.GetAsync($"/api/books/{book.Id}");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<BookDto>();
            Assert.That(result!.Id, Is.EqualTo(book.Id));
        }

        [Test]
        public async Task GetById_WhenBookDoesNotExist_ReturnsNotFound()
        {
            AuthorizeAsCrudClient();

            var response = await _client.GetAsync("/api/books/999");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task Create_WhenValid_ReturnsCreated()
        {
            AuthorizeAsCrudClient();
            var author = SeedAuthor();
            var dto = new CreateBookDto { AuthorId = author.Id, Title = "New Book" };

            var response = await _client.PostAsJsonAsync("/api/books", dto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var created = await response.Content.ReadFromJsonAsync<BookDto>();
            Assert.That(created!.Title, Is.EqualTo(dto.Title));
        }

        [Test]
        public async Task Create_WhenAuthorDoesNotExist_ReturnsBadRequest()
        {
            AuthorizeAsCrudClient();
            var dto = new CreateBookDto { AuthorId = 999, Title = "New Book" };

            var response = await _client.PostAsJsonAsync("/api/books", dto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task Create_WhenTitleIsTooShort_ReturnsBadRequest()
        {
            AuthorizeAsCrudClient();
            var author = SeedAuthor();
            var dto = new CreateBookDto { AuthorId = author.Id, Title = "ab" };

            var response = await _client.PostAsJsonAsync("/api/books", dto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task Update_WhenValid_ReturnsNoContent()
        {
            AuthorizeAsCrudClient();
            var author = SeedAuthor();
            var book = SeedBook(author.Id);
            var dto = new UpdateBookDto { AuthorId = author.Id, Title = "Updated Title" };

            var response = await _client.PutAsJsonAsync($"/api/books/{book.Id}", dto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public async Task Update_WhenBookDoesNotExist_ReturnsNotFound()
        {
            AuthorizeAsCrudClient();
            var author = SeedAuthor();
            var dto = new UpdateBookDto { AuthorId = author.Id, Title = "Updated Title" };

            var response = await _client.PutAsJsonAsync("/api/books/999", dto);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task Delete_WhenBookExists_ReturnsNoContent()
        {
            AuthorizeAsCrudClient();
            var author = SeedAuthor();
            var book = SeedBook(author.Id);

            var response = await _client.DeleteAsync($"/api/books/{book.Id}");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        }

        [Test]
        public async Task Delete_WhenBookDoesNotExist_ReturnsNotFound()
        {
            AuthorizeAsCrudClient();

            var response = await _client.DeleteAsync("/api/books/999");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
    }
}
