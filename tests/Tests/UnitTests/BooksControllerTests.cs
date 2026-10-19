using API.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.UnitTests
{
    [TestFixture]
    public class BooksControllerTests
    {
        private Mock<IBookService> _bookServiceMock = null!;
        private BooksController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _bookServiceMock = new Mock<IBookService>();
            _sut = new BooksController(_bookServiceMock.Object);
        }

        [Test]
        public async Task GetAll_ReturnsOkWithBooks()
        {
            var books = new List<BookDto>
            {
                new() { Id = 1, AuthorId = 1, Title = "Book One" },
                new() { Id = 2, AuthorId = 1, Title = "Book Two" }
            };

            _bookServiceMock
                .Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(books);

            var result = await _sut.GetAll(CancellationToken.None);

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(books));
        }

        [Test]
        public async Task GetById_WhenBookExists_ReturnsOkWithBook()
        {
            var book = new BookDto { Id = 1, AuthorId = 1, Title = "Book One" };

            _bookServiceMock
                .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(book);

            var result = await _sut.GetById(1, CancellationToken.None);

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(book));
        }

        [Test]
        public async Task GetById_WhenBookDoesNotExist_ReturnsNotFound()
        {
            _bookServiceMock
                .Setup(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BookDto?)null);

            var result = await _sut.GetById(999, CancellationToken.None);

            Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task Create_WhenValid_ReturnsCreatedAtAction()
        {
            var dto = new CreateBookDto { AuthorId = 1, Title = "New Book" };
            var created = new BookDto { Id = 1, AuthorId = 1, Title = "New Book" };

            _bookServiceMock
                .Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

            var result = await _sut.Create(dto, CancellationToken.None);

            var createdResult = result.Result as CreatedAtActionResult;
            Assert.That(createdResult, Is.Not.Null);
            Assert.That(createdResult!.Value, Is.EqualTo(created));
            Assert.That(createdResult.ActionName, Is.EqualTo(nameof(BooksController.GetById)));
        }

        [Test]
        public async Task Create_WhenAuthorDoesNotExist_ReturnsBadRequest()
        {
            var dto = new CreateBookDto { AuthorId = 999, Title = "New Book" };

            _bookServiceMock
                .Setup(s => s.CreateAsync(dto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Author with ID 999 does not exist."));

            var result = await _sut.Create(dto, CancellationToken.None);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Update_WhenBookExists_ReturnsNoContent()
        {
            var dto = new UpdateBookDto { AuthorId = 1, Title = "Updated" };

            _bookServiceMock
                .Setup(s => s.UpdateAsync(1, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _sut.Update(1, dto, CancellationToken.None);

            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public async Task Update_WhenBookDoesNotExist_ReturnsNotFound()
        {
            var dto = new UpdateBookDto { AuthorId = 1, Title = "Updated" };

            _bookServiceMock
                .Setup(s => s.UpdateAsync(999, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _sut.Update(999, dto, CancellationToken.None);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task Update_WhenAuthorDoesNotExist_ReturnsBadRequest()
        {
            var dto = new UpdateBookDto { AuthorId = 999, Title = "Updated" };

            _bookServiceMock
                .Setup(s => s.UpdateAsync(1, dto, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Author with ID 999 does not exist."));

            var result = await _sut.Update(1, dto, CancellationToken.None);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Delete_WhenBookExists_ReturnsNoContent()
        {
            _bookServiceMock
                .Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _sut.Delete(1, CancellationToken.None);

            Assert.That(result, Is.TypeOf<NoContentResult>());
        }

        [Test]
        public async Task Delete_WhenBookDoesNotExist_ReturnsNotFound()
        {
            _bookServiceMock
                .Setup(s => s.DeleteAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _sut.Delete(999, CancellationToken.None);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }
    }
}
