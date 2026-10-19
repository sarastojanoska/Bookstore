using System.Net;
using System.Net.Http.Json;
using Application.DTOs;
using Domain.Entities;

namespace Tests.IntegrationTests
{
    [TestFixture]
    public class BookSearchControllerIntegrationTests
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

        private void AuthorizeAsSearchClient()
        {
            _client.DefaultRequestHeaders.Add(TestAuthHandler.ScopeHeader, "books.search");
        }

        private Author SeedAuthor(string name)
        {
            Author author = null!;

            _factory.SeedDatabase(context =>
            {
                author = new Author { Name = name };
                context.Authors.Add(author);
            });

            return author;
        }

        private void SeedBook(int authorId, string title)
        {
            _factory.SeedDatabase(context =>
            {
                context.Books.Add(new Book { AuthorId = authorId, Title = title });
            });
        }

        [Test]
        public async Task Search_WithoutAuthentication_ReturnsUnauthorized()
        {
            var response = await _client.GetAsync("/api/booksearch");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test]
        public async Task Search_WhenAuthorized_ReturnsMatchingBooks()
        {
            AuthorizeAsSearchClient();
            var author = SeedAuthor("Jane Austen");
            SeedBook(author.Id, "Pride and Prejudice");
            SeedBook(author.Id, "Emma");

            var response = await _client.GetAsync("/api/booksearch?title=Pride");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<PagedResult<BookDto>>();
            Assert.That(result!.Items, Has.Count.EqualTo(1));
            Assert.That(result.Items[0].Title, Is.EqualTo("Pride and Prejudice"));
        }

        [Test]
        public async Task Search_WhenPageNumberInvalid_ReturnsBadRequest()
        {
            AuthorizeAsSearchClient();

            var response = await _client.GetAsync("/api/booksearch?pageNumber=0");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task Search_WhenPageSizeInvalid_ReturnsBadRequest()
        {
            AuthorizeAsSearchClient();

            var response = await _client.GetAsync("/api/booksearch?pageSize=101");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task Search_ReturnsPagedResults()
        {
            AuthorizeAsSearchClient();
            var author = SeedAuthor("Jane Austen");
            for (var i = 1; i <= 5; i++)
            {
                SeedBook(author.Id, $"Book {i}");
            }

            var response = await _client.GetAsync("/api/booksearch?pageNumber=2&pageSize=2");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var result = await response.Content.ReadFromJsonAsync<PagedResult<BookDto>>();
            Assert.That(result!.Items, Has.Count.EqualTo(2));
            Assert.That(result.TotalCount, Is.EqualTo(5));
        }
    }
}
