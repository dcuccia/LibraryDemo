using Library.WebApi.Models;
using Library.WebApi.Models.FailureResponses;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Library.WebApi.Tests;

public class LibraryEndpointsTests : IClassFixture<WebApplicationFactory<IApiMarker>>
{
    private readonly WebApplicationFactory<IApiMarker> _factory;

    public LibraryEndpointsTests(WebApplicationFactory<IApiMarker> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateBook_CreatesBook_WhenDataIsCorrect()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook();

        // Act
        var result = await httpClient.PostAsJsonAsync("/books", book);
        var createdBook = await result.Content.ReadFromJsonAsync<Book>();
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        createdBook.Should().BeEquivalentTo(book);
        result.Headers.Location.Should().Be($"http://localhost/books/{book.Isbn}");
    }

    [Fact]
    public async Task CreateBook_Fails_WhenIsbnIsInvalid()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook() with { Isbn = "INVALID" };
        
        // Act
        var result = await httpClient.PostAsJsonAsync("/books", book);
        var validationFailureResponse = await result.Content.ReadFromJsonAsync<ValidationFailureResponse>();
        var error = validationFailureResponse!.Errors.Single();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.ProperyName.Should().Be("Isbn");
        error.ErrorMessage.Should().Be("Value was not a valid ISBN-13");
    }

    [Fact]
    public async Task CreateBook_Fails_WhenBookExists()
    {
        // Arrange
        var httpClient = _factory.CreateClient();
        var book = GenerateBook() with { Isbn = "300-1234567890" };
        
        // Act
        await httpClient.PostAsJsonAsync("/books", book);
        var result = await httpClient.PostAsJsonAsync("/books", book);
        var validationFailureResponse = await result.Content.ReadFromJsonAsync<CreationFailureResponse>();
        var error = validationFailureResponse!.Errors.Single();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Should().Be("Book not added, because it already exists in the library");
    }

    private Book GenerateBook(string title = "The Eye of the World")
        => new Book
        {
            Isbn = GenerateIsbn(),
            Title = title,
            Author = "Robert Jordan",
            PageCount = 1337,
            ShortDescription = "Great first-book world-building with somewhat shaky canon.",
            ReleaseDate = new DateTime(1990, 1, 15)
        };

    private string GenerateIsbn()
        => $"{Random.Shared.Next(100, 999)}-" + $"{Random.Shared.Next(1000000000, 2100999999)}";
}