using API.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.UnitTests
{
    [TestFixture]
    public class BookSearchControllerTests
    {
        private Mock<IBookService> _bookServiceMock = null!;
        private BookSearchController _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _bookServiceMock = new Mock<IBookService>();
            _sut = new BookSearchController(_bookServiceMock.Object);
        }

        [Test]
        public async Task Search_ReturnsOkWithPagedResult()
        {
            var pagedResult = new PagedResult<BookDto>
            {
                Items = [new BookDto { Id = 1, AuthorId = 1, Title = "Book One" }],
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 1
            };

            _bookServiceMock
                .Setup(s => s.SearchAsync("Book", null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

            var result = await _sut.Search("Book", null, 1, 10, CancellationToken.None);

            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.Value, Is.EqualTo(pagedResult));
        }

        [Test]
        public async Task Search_WhenPageNumberInvalid_ReturnsBadRequest()
        {
            _bookServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<string?>(), It.IsAny<string?>(), 0, 10, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentOutOfRangeException("pageNumber", "Page number must be greater than 0."));

            var result = await _sut.Search(null, null, 0, 10, CancellationToken.None);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Search_WhenPageSizeInvalid_ReturnsBadRequest()
        {
            _bookServiceMock
                .Setup(s => s.SearchAsync(It.IsAny<string?>(), It.IsAny<string?>(), 1, 101, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentOutOfRangeException("pageSize", "Page size must be between 1 and 100."));

            var result = await _sut.Search(null, null, 1, 101, CancellationToken.None);

            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
        }
    }
}
